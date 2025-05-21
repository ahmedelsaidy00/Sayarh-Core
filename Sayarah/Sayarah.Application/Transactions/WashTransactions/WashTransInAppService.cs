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
    public class WashTransInAppService : AsyncCrudAppService<WashTransIn, WashTransInDto, long, GetWashTransInsInput, CreateWashTransInDto, UpdateWashTransInDto>, IWashTransInAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IRepository<Veichle, long> _veichleRepository;


        public WashTransInAppService(
            IRepository<WashTransIn, long> repository,
             ICommonAppService commonService,
             IRepository<Veichle, long> veichleRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _veichleRepository = veichleRepository;
        }

        public override async Task<WashTransInDto> GetAsync(EntityDto<long> input)
        {
            var WashTransIn = await Repository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(WashTransIn);
        }

        [AbpAuthorize]
        public override async Task<WashTransInDto> CreateAsync(CreateWashTransInDto input)
        {
            try
            {
                //Check if washTransIn exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "WashTransIns", CodeField = "Code" });
                var washTransIn = ObjectMapper.Map<WashTransIn>(input);
                washTransIn = await Repository.InsertAsync(washTransIn);
                await CurrentUnitOfWork.SaveChangesAsync();

                await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

                return MapToEntityDto(washTransIn);
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
              
                var Wash_In = await Repository.GetAll()
                                       .Where(a => a.VeichleId == input.Id && a.IsDeleted == false)
                                       .SumAsync(a => (decimal?)a.Quantity) ?? 0;

                _veichel.Wash_In = Convert.ToDecimal(Wash_In);
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
        public override async Task<WashTransInDto> UpdateAsync(UpdateWashTransInDto input)
        {

            var washTransIn = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, washTransIn);
            await Repository.UpdateAsync(washTransIn);
            await UnitOfWorkManager.Current.SaveChangesAsync();
            await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

            return MapToEntityDto(washTransIn);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var washTransIn = await Repository.GetAsync(input.Id);
            if (washTransIn == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");

            long VeichleId = washTransIn.VeichleId.Value;
            await Repository.DeleteAsync(washTransIn);


            await UpdateVeichleQuantities(new EntityDto<long> { Id = VeichleId });

        }

        public override async Task<PagedResultDto<WashTransInDto>> GetAllAsync(GetWashTransInsInput input)
        {
            var query = Repository.GetAll().Include(at => at.Branch.Company).Include(at => at.Veichle).AsQueryable();
            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
            query = query.WhereIf(input.Quantity.HasValue, at => at.Quantity == input.Quantity);
            query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);

            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var washTransIns = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<WashTransInDto>>(washTransIns);
            return new PagedResultDto<WashTransInDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<WashTransInDto>> GetPaged(GetWashTransInsPagedInput input)
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
                            WashTransIn washTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (washTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _washTransInClinicRepository.CountAsync(a => a.WashTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.WashTransIns.HasClinics"));

                                    long VeichleId = washTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(washTransIn);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                    await UpdateVeichleQuantities(new EntityDto<long> { Id = VeichleId });


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
                            WashTransIn washTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (washTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _washTransInClinicRepository.CountAsync(a => a.WashTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.WashTransIns.HasClinics"));

                                    long VeichleId = washTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(washTransIn);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                    await UpdateVeichleQuantities(new EntityDto<long> { Id = VeichleId });

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll().Include(a => a.Branch.Company).Include(a => a.Veichle).AsQueryable();

                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);

                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.Quantity.HasValue, at => at.Quantity == input.Quantity);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);


                    int filteredCount = await query.CountAsync();
                    var washTransIns = await query.Include(x => x.Branch)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<WashTransInDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<WashTransInDto>>(washTransIns)
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
