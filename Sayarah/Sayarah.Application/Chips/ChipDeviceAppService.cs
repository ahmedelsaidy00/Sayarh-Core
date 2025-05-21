using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Chips.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Authorization.Users;
using Sayarah.Chips;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Chips;

[DisableAuditing]
public class ChipDeviceAppService : AsyncCrudAppService<ChipDevice, ChipDeviceDto, long, GetAllChipDevices, CreateChipDeviceDto, UpdateChipDeviceDto>, IChipDeviceAppService
{
    private readonly IRepository<ChipDevice, long> _chipDeviceRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly IRepository<ChipNumber, long> _chipNumberRepository;
    public ChipDeviceAppService(
        IRepository<ChipDevice, long> chipDeviceRepository,
        IRepository<User, long> userRepository,
        IRepository<ChipNumber, long> chipNumberRepository)
        : base(chipDeviceRepository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _chipDeviceRepository = chipDeviceRepository;
        _userRepository = userRepository;
        _chipNumberRepository = chipNumberRepository;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<ChipDeviceDto>> GetPaged(GetChipDevicesInput input)
    {
        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
        {
            var ids = input.ids?.Select(long.Parse).ToList() ?? new List<long>();

            if (input.actionType == "GroupAction" && input.action == "Delete")
            {
                foreach (var id in ids)
                {
                    await HandleChipDeviceDeleteAsync(id);
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            else if (input.actionType == "SingleAction" && ids.Count != 0)
            {
                var id = ids.First();
                if (input.action == "Delete")
                {
                    await HandleChipDeviceDeleteAsync(id);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            // Fix: Separate the variable for the Include and use queryable for further filtering
            var baseQuery = _chipDeviceRepository.GetAll().Include(x => x.CreatorUser);

            var query = baseQuery
                .WhereIf(!string.IsNullOrEmpty(input.NameAr), x => x.NameAr.Contains(input.NameAr))
                .WhereIf(!string.IsNullOrEmpty(input.NameEn), x => x.NameEn.Contains(input.NameEn))
                .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.Contains(input.Code))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
                .FilterDataTable(input);

            var totalCount = await _chipDeviceRepository.CountAsync();
            var filteredCount = await query.CountAsync();

            var orderBy = $"{input.columns[input.order[0].column].name} {input.order[0].dir}";
            var chipDevices = await query
                .OrderBy(orderBy)
                .Skip(input.start)
                .Take(input.length)
                .ToListAsync();

            return new DataTableOutputDto<ChipDeviceDto>
            {
                iTotalRecords = totalCount,
                iTotalDisplayRecords = filteredCount,
                aaData = ObjectMapper.Map<List<ChipDeviceDto>>(chipDevices)
            };
        }
    }
    private async Task HandleChipDeviceDeleteAsync(long chipDeviceId)
    {
        var chipDevice = await _chipDeviceRepository.FirstOrDefaultAsync(chipDeviceId);
        if (chipDevice == null) return;

        var attachedChipNumbers = await _chipNumberRepository.GetAll()
            .Where(cn => cn.ChipDeviceId == chipDeviceId)
            .ToListAsync();

        if (attachedChipNumbers.Any())
        {
            foreach (var chip in attachedChipNumbers)
            {
                chip.ChipDeviceId = null;
                await _chipNumberRepository.UpdateAsync(chip);
            }
        }

        await _chipDeviceRepository.DeleteAsync(chipDevice);
    }
    public override async Task<ChipDeviceDto> GetAsync(EntityDto<long> input)
    {
        var chipDevice = await _chipDeviceRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
        return ObjectMapper.Map<ChipDeviceDto>(chipDevice);
    }

    [AbpAuthorize]
    public override async Task<ChipDeviceDto> CreateAsync(CreateChipDeviceDto input)
    {
        try
        {
            var exists = await _chipDeviceRepository.CountAsync(x => x.NameAr == input.NameAr && x.NameEn == input.NameEn) > 0;
            if (exists)
                throw new UserFriendlyException(L("Pages.ChipDevices.Error.AlreadyExist"));

            var chipDevice = ObjectMapper.Map<ChipDevice>(input);
            await _chipDeviceRepository.InsertAsync(chipDevice);
            return MapToEntityDto(chipDevice);
        }
        catch
        {
            throw;
        }
    }

    [AbpAuthorize]
    public override async Task<ChipDeviceDto> UpdateAsync(UpdateChipDeviceDto input)
    {
        try
        {
            var exists = await _chipDeviceRepository.CountAsync(x =>
                x.NameAr == input.NameAr && x.NameEn == input.NameEn && x.Id != input.Id) > 0;

            if (exists)
                throw new UserFriendlyException(L("Pages.ChipDevices.Error.AlreadyExist"));

            var chipDevice = await _chipDeviceRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, chipDevice);
            await _chipDeviceRepository.UpdateAsync(chipDevice);
            return MapToEntityDto(chipDevice);
        }
        catch
        {
            throw;
        }
    }
    public override async Task<PagedResultDto<ChipDeviceDto>> GetAllAsync(GetAllChipDevices input)
    {
        try
        {
            var query = _chipDeviceRepository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Name), x => x.NameAr.Contains(input.Name) || x.NameEn.Contains(input.Name));

            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = await query.CountAsync();
            }

            var chipDevices = await query
                .OrderBy(x => x.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var total = await query.CountAsync();

            return new PagedResultDto<ChipDeviceDto>(total, ObjectMapper.Map<List<ChipDeviceDto>>(chipDevices));
        }
        catch
        {
            throw;
        }
    }
}
