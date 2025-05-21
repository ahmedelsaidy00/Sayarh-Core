using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Transactions;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Transactions.FuelTransactions
{
    public class FuelTransInAppService : AsyncCrudAppService<FuelTransIn, FuelTransInDto, long, GetFuelTransInsInput, CreateFuelTransInDto, UpdateFuelTransInDto>, IFuelTransInAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IRepository<Veichle, long> _veichleRepository;


        public FuelTransInAppService(
            IRepository<FuelTransIn, long> repository,
             ICommonAppService commonService,
             IRepository<Veichle, long> veichleRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _veichleRepository = veichleRepository;
        }

        public override async Task<FuelTransInDto> GetAsync(EntityDto<long> input)
        {
            var FuelTransIn = await Repository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(FuelTransIn);
        }

        [AbpAuthorize]
        public override async Task<FuelTransInDto> CreateAsync(CreateFuelTransInDto input)
        {
            try
            {
                //Check if fuelTransIn exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "FuelTransIns", CodeField = "Code" });
                var fuelTransIn = ObjectMapper.Map<FuelTransIn>(input);
                fuelTransIn = await Repository.InsertAsync(fuelTransIn);
                await CurrentUnitOfWork.SaveChangesAsync();

                await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

                return MapToEntityDto(fuelTransIn);
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
                // update Veichle fuel trans in 
                var _veichel = await _veichleRepository.FirstOrDefaultAsync(input.Id);

                var Fuel_In = await Repository.GetAll()
                                       .Where(a => a.VeichleId == input.Id && a.IsDeleted == false)
                                       .SumAsync(a => (decimal?)a.Quantity) ?? 0;

                _veichel.Fuel_In = Convert.ToDecimal(Fuel_In);
                _veichel.Fuel_Balance = _veichel.Fuel_In - _veichel.Fuel_Out;

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
        public override async Task<FuelTransInDto> UpdateAsync(UpdateFuelTransInDto input)
        {

            var fuelTransIn = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, fuelTransIn);
            await Repository.UpdateAsync(fuelTransIn);
            await UnitOfWorkManager.Current.SaveChangesAsync();
            await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

            return MapToEntityDto(fuelTransIn);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var fuelTransIn = await Repository.GetAsync(input.Id);
            if (fuelTransIn == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");

            long VeichleId = fuelTransIn.VeichleId.Value;
            await Repository.DeleteAsync(fuelTransIn);


            await UpdateVeichleQuantities(new EntityDto<long> { Id = VeichleId });

        }

        public override async Task<PagedResultDto<FuelTransInDto>> GetAllAsync(GetFuelTransInsInput input)
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

            var fuelTransIns = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<FuelTransInDto>>(fuelTransIns);
            return new PagedResultDto<FuelTransInDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<FuelTransInDto>> GetPaged(GetFuelTransInsPagedInput input)
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
                            FuelTransIn fuelTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (fuelTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _fuelTransInClinicRepository.CountAsync(a => a.FuelTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.FuelTransIns.HasClinics"));

                                    long VeichleId = fuelTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(fuelTransIn);
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
                            FuelTransIn fuelTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (fuelTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _fuelTransInClinicRepository.CountAsync(a => a.FuelTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.FuelTransIns.HasClinics"));

                                    long VeichleId = fuelTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(fuelTransIn);
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
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);

                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.Quantity.HasValue, at => at.Quantity == input.Quantity);

                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);


                    int filteredCount = await query.CountAsync();
                    var fuelTransIns = await query.Include(x => x.Branch)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<FuelTransInDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<FuelTransInDto>>(fuelTransIns)
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
