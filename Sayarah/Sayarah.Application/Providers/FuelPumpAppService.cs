using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Users;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Providers;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Providers;

[AbpAuthorize]
public class FuelPumpAppService : AsyncCrudAppService<FuelPump, FuelPumpDto, long, GetFuelPumpsInput, CreateFuelPumpDto, UpdateFuelPumpDto>, IFuelPumpAppService
{
    private readonly IUserAppService _userService;
    private readonly UserManager _userManager;
    private readonly IRepository<User, long> _userRepository;
    private readonly IRepository<UserDashboard, long> _userDashboardRepository;
    private readonly ICommonAppService _commonService;
    private readonly RoleManager _roleManager;

    public FuelPumpAppService(
        IRepository<FuelPump, long> repository,
        IUserAppService userService,
        UserManager userManager,
        ICommonAppService commonService,
        IRepository<User, long> userRepository,
        IRepository<UserDashboard, long> userDashboardRepository,
        RoleManager roleManager
    )
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _userService = userService;
        _userManager = userManager;
        _commonService = commonService;
        _userRepository = userRepository;
        _userDashboardRepository = userDashboardRepository;
        _roleManager = roleManager;
    }

    public override async Task<FuelPumpDto> GetAsync(EntityDto<long> input)
    {
        var fuelPump = await Repository.GetAll()
            .Include(x => x.Provider.MainProvider)
            .FirstOrDefaultAsync(x => x.Id == input.Id);

        var mapped = ObjectMapper.Map<FuelPumpDto>(fuelPump);

        if (mapped != null)
        {
            mapped.QrCode = GetQrCode(mapped.Code);
        }

        return mapped;
    }

    public override async Task<FuelPumpDto> CreateAsync(CreateFuelPumpDto input)
    {
        //Check if fuelPump exists
        int existingCount = await Repository.CountAsync(at => at.Code == input.Code && at.ProviderId == input.ProviderId);
        if (existingCount > 0)
            throw new UserFriendlyException(L("Pages.FuelPumps.Error.AlreadyExist"));

        var fuelPump = ObjectMapper.Map<FuelPump>(input);
        fuelPump = await Repository.InsertAsync(fuelPump);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEntityDto(fuelPump);
    }

    public async Task<bool> CreateMultiple(CreateMultipleFuelPumpDto input)
    {
        if (input.Count > 0)
        {
            for (int i = 0; i < input.Count; i++)
            {
                Guid code = Guid.NewGuid();
                await CreateAsync(new CreateFuelPumpDto
                {
                    Code = code.ToString(),
                    ProviderId = input.ProviderId
                });
            }
        }
        return true;
    }

    public override async Task<FuelPumpDto> UpdateAsync(UpdateFuelPumpDto input)
    {
        int existingCount = await Repository.CountAsync(at => at.Id != input.Id && at.Code == input.Code && at.ProviderId == input.ProviderId);
        if (existingCount > 0)
            throw new UserFriendlyException(L("Pages.FuelPumps.Error.AlreadyExist"));

        var fuelPump = await Repository.GetAllIncluding(x => x.Provider.MainProvider).FirstOrDefaultAsync(x => x.Id == input.Id);
        ObjectMapper.Map(input, fuelPump);
        await Repository.UpdateAsync(fuelPump);
        return MapToEntityDto(fuelPump);
    }

    public override async Task DeleteAsync(EntityDto<long> input)
    {
        var fuelPump = await Repository.GetAsync(input.Id);
        if (fuelPump == null)
            throw new UserFriendlyException("Common.Message.ElementNotFound");
        await Repository.DeleteAsync(fuelPump);
    }

    public override async Task<PagedResultDto<FuelPumpDto>> GetAllAsync(GetFuelPumpsInput input)
    {
        var query = Repository.GetAll().Include(at => at.Provider.MainProvider).AsQueryable();
        query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
        query = query.WhereIf(input.MainProviderId.HasValue, a => a.Provider.MainProviderId == input.MainProviderId);
        int count = query.Count();
        if (input.MaxCount == true)
        {
            input.SkipCount = 0;
            input.MaxResultCount = query.Count();
        }

        var fuelPumps = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
        var mappedList = ObjectMapper.Map<List<FuelPumpDto>>(fuelPumps);
        return new PagedResultDto<FuelPumpDto>(count, mappedList);
    }

    public async Task<DataTableOutputDto<FuelPumpDto>> GetPaged(GetFuelPumpsPagedInput input)
    {
        using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
        {
            int id = 0;
            if (input.actionType == "GroupAction")
            {
                for (int i = 0; i < input.ids.Length; i++)
                {
                    id = Convert.ToInt32(input.ids[i]);
                    FuelPump fuelPump = await Repository.GetAllIncluding(a => a.Provider.MainProvider).FirstOrDefaultAsync(a => a.Id == id);
                    if (fuelPump != null && input.action == "Delete")
                    {
                        fuelPump.IsDeleted = true;
                        fuelPump.DeletionTime = DateTime.Now;
                        fuelPump.DeleterUserId = AbpSession.UserId;
                        await Repository.UpdateAsync(fuelPump);
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                }
                await UnitOfWorkManager.Current.SaveChangesAsync();
            }
            else if (input.actionType == "SingleAction")
            {
                if (input.ids.Length > 0)
                {
                    id = Convert.ToInt32(input.ids[0]);
                    FuelPump fuelPump = await Repository.GetAllIncluding(a => a.Provider.MainProvider).FirstOrDefaultAsync(a => a.Id == id);
                    if (fuelPump != null && input.action == "Delete")
                    {
                        fuelPump.IsDeleted = true;
                        fuelPump.DeletionTime = DateTime.Now;
                        fuelPump.DeleterUserId = AbpSession.UserId;
                        await Repository.UpdateAsync(fuelPump);
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                }
                await UnitOfWorkManager.Current.SaveChangesAsync();
            }
            var query = Repository.GetAll()
                .Include(a => a.Provider.MainProvider)
                .Where(at => at.ProviderId == input.ProviderId);

            query = query.WhereIf(input.MainProviderId.HasValue, a => a.Provider.MainProviderId == input.MainProviderId);

            int count = await query.CountAsync();
            query = query.FilterDataTable((DataTableInputDto)input);
            query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
            query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);
            int filteredCount = await query.CountAsync();
            var fuelPumps = await query.Include(x => x.Provider.MainProvider)
                .Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                .Skip(input.start).Take(input.length).ToListAsync();

            var mappedList = ObjectMapper.Map<List<FuelPumpDto>>(fuelPumps);

            if (mappedList != null)
            {
                foreach (var item in mappedList.ToList())
                {
                    item.QrCode = GetQrCode(item.Code);
                }
            }
            return new DataTableOutputDto<FuelPumpDto>
            {
                iTotalDisplayRecords = filteredCount,
                iTotalRecords = count,
                aaData = mappedList
            };
        }
    }

    public string GetQrCode(string input)
    {
        using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(input, QRCodeGenerator.ECCLevel.M);
            var qrCodeImage = new Base64QRCode(qrCodeData);
            var qrCodeImageSrc = qrCodeImage.GetGraphic(20);
            var qrCodeImageSrcString = "data:image/png;base64," + qrCodeImageSrc;
            return qrCodeImageSrcString;
        }
    }

    public async Task<List<GenerateQrCodeListOutput>> GenerateQrCodeList(GenerateQrCodeList input)
    {
        List<GenerateQrCodeListOutput> qrCodes = new List<GenerateQrCodeListOutput>();
        foreach (var id in input.Ids)
        {
            var pump = await Repository.FirstOrDefaultAsync(id);
            string code = GetQrCode(pump.Code);
            qrCodes.Add(new GenerateQrCodeListOutput { Code = pump.Code, QrCode = code });
        }
        return qrCodes;
    }
}
