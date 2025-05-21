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
    public class OilTransInAppService : AsyncCrudAppService<OilTransIn, OilTransInDto, long, GetOilTransInsInput, CreateOilTransInDto, UpdateOilTransInDto>, IOilTransInAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IRepository<Veichle, long> _veichleRepository;


        public OilTransInAppService(
            IRepository<OilTransIn, long> repository,
             ICommonAppService commonService,
             IRepository<Veichle, long> veichleRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _veichleRepository = veichleRepository;
        }

        public override async Task<OilTransInDto> GetAsync(EntityDto<long> input)
        {
            var OilTransIn = await Repository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(OilTransIn);
        }

        [AbpAuthorize]
        public override async Task<OilTransInDto> CreateAsync(CreateOilTransInDto input)
        {
            try
            {
                //Check if oilTransIn exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "OilTransIns", CodeField = "Code" });
                var oilTransIn = ObjectMapper.Map<OilTransIn>(input);
                oilTransIn = await Repository.InsertAsync(oilTransIn);
                await CurrentUnitOfWork.SaveChangesAsync();

                await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

                return MapToEntityDto(oilTransIn);
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

                var Oil_In = await Repository.GetAll()
                                       .Where(a => a.VeichleId == input.Id && a.IsDeleted == false)
                                       .SumAsync(a => (decimal?)a.Quantity) ?? 0;

                _veichel.Oil_In = Convert.ToDecimal(Oil_In);
                _veichel.Oil_Balance = _veichel.Oil_In - _veichel.Oil_Out;

                await _veichleRepository.UpdateAsync(_veichel);

                await UnitOfWorkManager.Current.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        [AbpAuthorize]
        public override async Task<OilTransInDto> UpdateAsync(UpdateOilTransInDto input)
        {

            var oilTransIn = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, oilTransIn);
            await Repository.UpdateAsync(oilTransIn);
            await UnitOfWorkManager.Current.SaveChangesAsync();
            await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

            return MapToEntityDto(oilTransIn);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var oilTransIn = await Repository.GetAsync(input.Id);
            if (oilTransIn == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");

            long VeichleId = oilTransIn.VeichleId.Value;
            await Repository.DeleteAsync(oilTransIn);


            await UpdateVeichleQuantities(new EntityDto<long> { Id = VeichleId });

        }

        public override async Task<PagedResultDto<OilTransInDto>> GetAllAsync(GetOilTransInsInput input)
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

            var oilTransIns = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<OilTransInDto>>(oilTransIns);
            return new PagedResultDto<OilTransInDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<OilTransInDto>> GetPaged(GetOilTransInsPagedInput input)
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
                            OilTransIn oilTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (oilTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _oilTransInClinicRepository.CountAsync(a => a.OilTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.OilTransIns.HasClinics"));

                                    long VeichleId = oilTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(oilTransIn);
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
                            OilTransIn oilTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (oilTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _oilTransInClinicRepository.CountAsync(a => a.OilTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.OilTransIns.HasClinics"));

                                    long VeichleId = oilTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(oilTransIn);
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
                    var oilTransIns = await query.Include(x => x.Branch)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<OilTransInDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<OilTransInDto>>(oilTransIns)
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
