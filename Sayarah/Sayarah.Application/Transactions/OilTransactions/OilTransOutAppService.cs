using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Transactions.OilTransactions.Dto;
using Sayarah.Transactions;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Transactions.OilTransactions
{
    public class OilTransOutAppService : AsyncCrudAppService<OilTransOut, OilTransOutDto, long, GetOilTransOutsInput, CreateOilTransOutDto, UpdateOilTransOutDto>, IOilTransOutAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IRepository<Veichle, long> _veichleRepository;


        public OilTransOutAppService(
            IRepository<OilTransOut, long> repository,
             ICommonAppService commonService,
             IRepository<Veichle, long> veichleRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _veichleRepository = veichleRepository;
        }

        public override async Task<OilTransOutDto> GetAsync(EntityDto<long> input)
        {
            var OilTransIn = await Repository.GetAll()
                .Include(at => at.Branch.Company)
                .Include(at => at.Driver)
                .Include(at => at.Provider.MainProvider)
                .Include(at => at.Worker)
                .Include(at => at.Veichle)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(OilTransIn);
        }

        [AbpAuthorize]
        public override async Task<OilTransOutDto> CreateAsync(CreateOilTransOutDto input)
        {
            try
            {
                //Check if oilTransOut exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "OilTransOuts", CodeField = "Code" });
                var oilTransOut = ObjectMapper.Map<OilTransOut>(input);
                oilTransOut = await Repository.InsertAsync(oilTransOut);
                await CurrentUnitOfWork.SaveChangesAsync();

                await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

                return MapToEntityDto(oilTransOut);
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
                // update Veichle oil trans in 
                var _veichel = await _veichleRepository.FirstOrDefaultAsync(input.Id);
               
                _veichel.Oil_Out = await Repository.GetAll().Where(a => a.VeichleId == input.Id).SumAsync(a => a.Quantity);
                _veichel.Oil_Balance = _veichel.Oil_In - _veichel.Oil_Out;

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
        public override async Task<OilTransOutDto> UpdateAsync(UpdateOilTransOutDto input)
        {

            var oilTransOut = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, oilTransOut);
            await Repository.UpdateAsync(oilTransOut);
            return MapToEntityDto(oilTransOut);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var oilTransOut = await Repository.GetAsync(input.Id);
            if (oilTransOut == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(oilTransOut);
        }

        public override async Task<PagedResultDto<OilTransOutDto>> GetAllAsync(GetOilTransOutsInput input)
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

            var oilTransIns = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<OilTransOutDto>>(oilTransIns);
            return new PagedResultDto<OilTransOutDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<OilTransOutDto>> GetPaged(GetOilTransOutsPagedInput input)
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
                            OilTransOut oilTransOut = await Repository.FirstOrDefaultAsync(id);
                            if (oilTransOut != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _oilTransInClinicRepository.CountAsync(a => a.OilTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.OilTransIns.HasClinics"));

                                    await Repository.DeleteAsync(oilTransOut);
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
                            OilTransOut oilTransOut = await Repository.FirstOrDefaultAsync(id);
                            if (oilTransOut != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _oilTransInClinicRepository.CountAsync(a => a.OilTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.OilTransIns.HasClinics"));

                                    await Repository.DeleteAsync(oilTransOut);
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
                    var oilTransIns = await query.Include(a => a.Branch.Company)
                        .Include(a => a.Driver)
                        .Include(a => a.Veichle)
                        .Include(a => a.Provider)
                        .Include(a => a.Worker)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<OilTransOutDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<OilTransOutDto>>(oilTransIns)
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