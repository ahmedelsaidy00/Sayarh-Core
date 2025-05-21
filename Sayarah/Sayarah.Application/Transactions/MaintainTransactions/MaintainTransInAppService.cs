using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Transactions.MaintainTransactions.Dto;
using Sayarah.Transactions;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Transactions.MaintainTransactions
{
    public class MaintainTransInAppService : AsyncCrudAppService<MaintainTransIn, MaintainTransInDto, long, GetMaintainTransInsInput, CreateMaintainTransInDto, UpdateMaintainTransInDto>, IMaintainTransInAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IRepository<Veichle, long> _veichleRepository;


        public MaintainTransInAppService(
            IRepository<MaintainTransIn, long> repository,
             ICommonAppService commonService,
             IRepository<Veichle, long> veichleRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _veichleRepository = veichleRepository;
        }

        public override async Task<MaintainTransInDto> GetAsync(EntityDto<long> input)
        {
            var MaintainTransIn = await Repository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(MaintainTransIn);
        }

        [AbpAuthorize]
        public override async Task<MaintainTransInDto> CreateAsync(CreateMaintainTransInDto input)
        {
            try
            {
                //Check if maintainTransIn exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "MaintainTransIns", CodeField = "Code" });
                var maintainTransIn = ObjectMapper.Map<MaintainTransIn>(input);
                maintainTransIn = await Repository.InsertAsync(maintainTransIn);
                await CurrentUnitOfWork.SaveChangesAsync();

                await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

                return MapToEntityDto(maintainTransIn);
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
                // update Veichle maintain trans in 
                var _veichel = await _veichleRepository.FirstOrDefaultAsync(input.Id);

                var Maintain_In = await Repository.GetAll()
                                       .Where(a => a.VeichleId == input.Id && a.IsDeleted == false)
                                       .SumAsync(a => (decimal?)a.Quantity) ?? 0;

                _veichel.Maintain_In = Convert.ToDecimal(Maintain_In);
                _veichel.Maintain_Balance = _veichel.Maintain_In - _veichel.Maintain_Out;

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
        public override async Task<MaintainTransInDto> UpdateAsync(UpdateMaintainTransInDto input)
        {

            var maintainTransIn = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, maintainTransIn);
            await Repository.UpdateAsync(maintainTransIn);
            await UnitOfWorkManager.Current.SaveChangesAsync();
            await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

            return MapToEntityDto(maintainTransIn);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var maintainTransIn = await Repository.GetAsync(input.Id);
            if (maintainTransIn == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");

            long VeichleId = maintainTransIn.VeichleId.Value;
            await Repository.DeleteAsync(maintainTransIn);


            await UpdateVeichleQuantities(new EntityDto<long> { Id = VeichleId });

        }

        public override async Task<PagedResultDto<MaintainTransInDto>> GetAllAsync(GetMaintainTransInsInput input)
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

            var maintainTransIns = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<MaintainTransInDto>>(maintainTransIns);
            return new PagedResultDto<MaintainTransInDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<MaintainTransInDto>> GetPaged(GetMaintainTransInsPagedInput input)
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
                            MaintainTransIn maintainTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (maintainTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _maintainTransInClinicRepository.CountAsync(a => a.MaintainTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.MaintainTransIns.HasClinics"));

                                    long VeichleId = maintainTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(maintainTransIn);
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
                            MaintainTransIn maintainTransIn = await Repository.FirstOrDefaultAsync(id);
                            if (maintainTransIn != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _maintainTransInClinicRepository.CountAsync(a => a.MaintainTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.MaintainTransIns.HasClinics"));

                                    long VeichleId = maintainTransIn.VeichleId.Value;

                                    await Repository.DeleteAsync(maintainTransIn);
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
                    var maintainTransIns = await query.Include(x => x.Branch)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<MaintainTransInDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<MaintainTransInDto>>(maintainTransIns)
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
