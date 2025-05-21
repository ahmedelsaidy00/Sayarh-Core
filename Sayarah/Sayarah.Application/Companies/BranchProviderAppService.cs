using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.Companies.Dto;
using Sayarah.Companies;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Sayarah.Application.Helpers;
using Sayarah.Application.DataTables.Dto;


namespace Sayarah.Application.Companies
{
    public class BranchProviderAppService : AsyncCrudAppService<BranchProvider, BranchProviderDto, long, GetBranchProviderProvidersInput, CreateBranchProviderDto, UpdateBranchProviderDto>, IBranchProviderAppService
    {

        //private readonly IUserAppService _userService;
        //private readonly UserManager _userManager;
        //private readonly IRepository<User, long> _userRepository;
        //private readonly ICommonAppService _commonService;
        public BranchProviderAppService(
            IRepository<BranchProvider, long> repository
            //IUserAppService userService,
            //UserManager userManager,
            //IRepository<User, long> userRepository,
            // ICommonAppService commonService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            //_userService = userService;
            //_userManager = userManager;
            //_userRepository = userRepository;
            //_commonService = commonService;
        }

        public override async Task<BranchProviderDto> GetAsync(EntityDto<long> input)
        {
            var BranchProvider = await Repository.GetAll()

                .Include(x => x.Branch.Company)
                .Include(x => x.Company)
                .Include(x => x.Provider.MainProvider)
                .Include(x => x.MainProvider)

                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(BranchProvider);
        }

        [AbpAuthorize]
        public override async Task<BranchProviderDto> CreateAsync(CreateBranchProviderDto input)
        {
            try
            {

                if (input.BranchId.HasValue == true)
                {
                    if (input.ProviderId.HasValue)
                    {
                        int existingCount = await Repository.CountAsync(at => at.BranchId == input.BranchId && at.ProviderId == input.ProviderId);
                        if (existingCount > 0)
                            throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistProvider"));
                    }
                    else if (input.MainProviderId.HasValue)
                    {
                        int existingCount = await Repository.CountAsync(at => at.BranchId == input.BranchId && at.ProviderId.HasValue == false && at.MainProviderId == input.MainProviderId);
                        if (existingCount > 0)
                            throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistMainProvider"));
                    }
                }
                else if (input.CompanyId.HasValue)
                {
                    if (input.ProviderId.HasValue)
                    {
                        int existingCount = await Repository.CountAsync(at => at.BranchId.HasValue == false && at.CompanyId == input.CompanyId && at.ProviderId == input.ProviderId);
                        if (existingCount > 0)
                            throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistProvider"));
                    }
                    else if (input.MainProviderId.HasValue)
                    {
                        int existingCount = await Repository.CountAsync(at => at.BranchId.HasValue == false && at.ProviderId.HasValue == false && at.CompanyId == input.CompanyId && at.MainProviderId == input.MainProviderId);
                        if (existingCount > 0)
                            throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistMainProvider"));
                    }
                }


                var branchProvider = ObjectMapper.Map<BranchProvider>(input);
                branchProvider = await Repository.InsertAsync(branchProvider);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(branchProvider);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public override async Task<BranchProviderDto> UpdateAsync(UpdateBranchProviderDto input)
        {

            if (input.BranchId.HasValue == true)
            {
                if (input.ProviderId.HasValue)
                {
                    int existingCount = await Repository.CountAsync(at => at.Id != input.Id && at.BranchId == input.BranchId && at.ProviderId == input.ProviderId);
                    if (existingCount > 0)
                        throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistProvider"));
                }
                else if (input.MainProviderId.HasValue)
                {
                    int existingCount = await Repository.CountAsync(at => at.Id != input.Id && at.BranchId == input.BranchId && at.MainProviderId == input.MainProviderId);
                    if (existingCount > 0)
                        throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistMainProvider"));
                }
            }
            else if (input.CompanyId.HasValue)
            {
                if (input.ProviderId.HasValue)
                {
                    int existingCount = await Repository.CountAsync(at => at.Id != input.Id && at.CompanyId == input.CompanyId && at.ProviderId == input.ProviderId);
                    if (existingCount > 0)
                        throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistProvider"));
                }
                else if (input.MainProviderId.HasValue)
                {
                    int existingCount = await Repository.CountAsync(at => at.Id != input.Id && at.CompanyId == input.CompanyId && at.MainProviderId == input.MainProviderId);
                    if (existingCount > 0)
                        throw new UserFriendlyException(L("Pages.Orders.FuelProvider.Errors.AlreadyExistMainProvider"));
                }
            }


            //int existingCount = await Repository.CountAsync(at => at.Id != input.Id && (at.BranchId == input.BranchId && at.ProviderId == input.ProviderId));
            //if (existingCount > 0)
            //    throw new UserFriendlyException(L("Pages.BranchProviders.Error.AlreadyExist"));

            var branchProvider = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);

            ObjectMapper.Map(input, branchProvider);
            await Repository.UpdateAsync(branchProvider);
            return MapToEntityDto(branchProvider);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var branchProvider = await Repository.GetAsync(input.Id);
            if (branchProvider == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(branchProvider);
        }

        public override async Task<PagedResultDto<BranchProviderDto>> GetAllAsync(GetBranchProviderProvidersInput input)
        {
            var query = Repository.GetAll()
                .Include(x => x.Branch.Company)
                .Include(x => x.Company)
                .Include(x => x.Provider.MainProvider)
                .Include(x => x.MainProvider).AsQueryable();


            query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId || at.CompanyId == input.CompanyId);
            query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId || at.MainProviderId == input.MainProviderId);


            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var branchProviderProviders = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<BranchProviderDto>>(branchProviderProviders);
            return new PagedResultDto<BranchProviderDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<BranchProviderDto>> GetPaged(GetBranchProviderProvidersPagedInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    int id = 0;
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            id = Convert.ToInt32(input.ids[i]);
                            BranchProvider branchProvider = await Repository.FirstOrDefaultAsync(id);
                            if (branchProvider != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _branchProviderClinicRepository.CountAsync(a => a.BranchProviderId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.BranchProviderProviders.HasClinics"));


                                    await Repository.DeleteAsync(branchProvider);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt32(input.ids[0]);
                            BranchProvider branchProvider = await Repository.FirstOrDefaultAsync(id);
                            if (branchProvider != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _branchProviderClinicRepository.CountAsync(a => a.BranchProviderId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.BranchProviderProviders.HasClinics"));

                                    await Repository.DeleteAsync(branchProvider);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }

                    if (input.CompanyId.HasValue == false)
                        return new DataTableOutputDto<BranchProviderDto>
                        {
                            iTotalDisplayRecords = 0,
                            iTotalRecords = 0,
                            aaData = new List<BranchProviderDto>()
                        };

                    var query = Repository.GetAll()
                         .Include(x => x.Branch.Company)
                .Include(x => x.Company)
                .Include(x => x.Provider.MainProvider)
                .Include(x => x.MainProvider).AsQueryable();

                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName));

                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId || at.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId || at.MainProviderId == input.MainProviderId);


                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);

                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);

                    int filteredCount = await query.CountAsync();

                    var branchProviderProviders = await query
                        .Include(a => a.Branch.Company)
                        .Include(a => a.Company)
                        .Include(a => a.Provider.MainProvider)
                        .Include(a => a.MainProvider)
                        .Include(x => x.CreatorUser)
                        .Include(x => x.LastModifierUser)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();

                    return new DataTableOutputDto<BranchProviderDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<BranchProviderDto>>(branchProviderProviders)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
