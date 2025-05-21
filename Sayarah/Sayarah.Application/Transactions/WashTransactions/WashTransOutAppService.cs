using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Transactions.WashTransactions.Dto;
using Sayarah.Transactions;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Transactions.WashTransactions
{
    public class WashTransOutAppService : AsyncCrudAppService<WashTransOut, WashTransOutDto, long, GetWashTransOutsInput, CreateWashTransOutDto, UpdateWashTransOutDto>, IWashTransOutAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IRepository<Veichle, long> _veichleRepository;


        public WashTransOutAppService(
            IRepository<WashTransOut, long> repository,
             ICommonAppService commonService,
             IRepository<Veichle, long> veichleRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _veichleRepository = veichleRepository;
        }

        public override async Task<WashTransOutDto> GetAsync(EntityDto<long> input)
        {
            var WashTransIn = await Repository.GetAll()
                .Include(at => at.Branch.Company)
                .Include(at => at.Driver)
                .Include(at => at.Provider.MainProvider)
                .Include(at => at.Worker)
                .Include(at => at.Veichle)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(WashTransIn);
        }

        [AbpAuthorize]
        public override async Task<WashTransOutDto> CreateAsync(CreateWashTransOutDto input)
        {
            try
            {
                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "WashTransOuts", CodeField = "Code" });
                var washTransOut = ObjectMapper.Map<WashTransOut>(input);
                washTransOut = await Repository.InsertAsync(washTransOut);
                await CurrentUnitOfWork.SaveChangesAsync();

                await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

                return MapToEntityDto(washTransOut);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<bool> UpdateVeichleQuantities(EntityDto<long> input)
        {
            try
            {
                // update Veichle wash trans in 
                var _veichel = await _veichleRepository.FirstOrDefaultAsync(input.Id);
               
                _veichel.Wash_Out = await Repository.GetAll().Where(a => a.VeichleId == input.Id).SumAsync(a => a.Quantity);
                _veichel.Wash_Balance = _veichel.Wash_In - _veichel.Wash_Out;

                await _veichleRepository.UpdateAsync(_veichel);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        [AbpAuthorize]
        public override async Task<WashTransOutDto> UpdateAsync(UpdateWashTransOutDto input)
        {

            var washTransOut = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, washTransOut);
            await Repository.UpdateAsync(washTransOut);
            return MapToEntityDto(washTransOut);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var washTransOut = await Repository.GetAsync(input.Id);
            if (washTransOut == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(washTransOut);
        }

        public override async Task<PagedResultDto<WashTransOutDto>> GetAllAsync(GetWashTransOutsInput input)
        {
            var query = Repository.GetAll()
                .Include(at => at.Branch.Company)
                .Include(at => at.Driver)
                .Include(at => at.Provider.MainProvider)
                .Include(at => at.Worker)
                .Include(at => at.Veichle).AsQueryable();
            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);  
            query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);  
            query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
            query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
            query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);

            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var washTransIns = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<WashTransOutDto>>(washTransIns);
            return new PagedResultDto<WashTransOutDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<WashTransOutDto>> GetPaged(GetWashTransOutsPagedInput input)
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
                            WashTransOut washTransOut = await Repository.FirstOrDefaultAsync(id);
                            if (washTransOut != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _washTransInClinicRepository.CountAsync(a => a.WashTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.WashTransIns.HasClinics"));

                                    await Repository.DeleteAsync(washTransOut);
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
                            WashTransOut washTransOut = await Repository.FirstOrDefaultAsync(id);
                            if (washTransOut != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _washTransInClinicRepository.CountAsync(a => a.WashTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.WashTransIns.HasClinics"));

                                    await Repository.DeleteAsync(washTransOut);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                        .Include(a => a.Branch.Company)
                        .Include(a => a.Driver)
                        .Include(a => a.Veichle)
                        .Include(a => a.Provider.MainProvider)
                        .Include(a => a.Worker).Where(a => a.Completed == true)
                        ; 
                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);
                    if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                        }
                        else
                            return new DataTableOutputDto<WashTransOutDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<WashTransOutDto>()
                            };
                    }

                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                    query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                    query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    
                    query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Branch.Company.NameAr.Contains(input.CompanyName) || at.Branch.Company.NameEn.Contains(input.CompanyName)  || at.Branch.Company.Code.Contains(input.CompanyName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName)  || at.Branch.Code.Contains(input.BranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName) || at.Driver.Code.Contains(input.DriverName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at => at.Veichle.NameAr.Contains(input.VeichleName) || at.Veichle.NameEn.Contains(input.VeichleName) || at.Veichle.PlateLetters.Contains(input.VeichleName) || at.Veichle.PlateLettersEn.Contains(input.VeichleName) || at.Veichle.PlateNumberEn.Contains(input.VeichleName) || at.Driver.Code.Contains(input.VeichleName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName)  || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName)  || at.Provider.Code.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName)  || at.Worker.Code.Contains(input.WorkerName));
                   
                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                    int filteredCount = await query.CountAsync();
                    var washTransIns = await query.Include(a => a.Branch.Company)
                        .Include(a => a.Driver)
                        .Include(a => a.Veichle)
                        .Include(a => a.Provider)
                        .Include(a => a.Worker)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<WashTransOutDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<WashTransOutDto>>(washTransIns)
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