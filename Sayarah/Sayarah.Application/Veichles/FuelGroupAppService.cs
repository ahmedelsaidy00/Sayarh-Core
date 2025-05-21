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
using Sayarah.Application.Users;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Veichles
{
    public class FuelGroupAppService : AsyncCrudAppService<FuelGroup, FuelGroupDto, long, GetFuelGroupsInput, CreateFuelGroupDto, UpdateFuelGroupDto>, IFuelGroupAppService
    {

        private readonly IUserAppService _userService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly ICommonAppService _commonService;

        public FuelGroupAppService(
            IRepository<FuelGroup, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
             ICommonAppService commonService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _commonService = commonService;
        }

        public override async Task<FuelGroupDto> GetAsync(EntityDto<long> input)
        {
            var FuelGroup = await Repository.GetAll()
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(FuelGroup);
        }


        [AbpAuthorize]
        public override async Task<FuelGroupDto> CreateAsync(CreateFuelGroupDto input)
        {
            try
            {
                //Check if fuelGroup exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "FuelGroups", CodeField = "Code" });
                var fuelGroup = ObjectMapper.Map<FuelGroup>(input);
                fuelGroup = await Repository.InsertAsync(fuelGroup);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(fuelGroup);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public override async Task<FuelGroupDto> UpdateAsync(UpdateFuelGroupDto input)
        {

            var fuelGroup = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, fuelGroup);
            await Repository.UpdateAsync(fuelGroup);
            return MapToEntityDto(fuelGroup);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var fuelGroup = await Repository.GetAsync(input.Id);
            if (fuelGroup == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(fuelGroup);
        }

        public override async Task<PagedResultDto<FuelGroupDto>> GetAllAsync(GetFuelGroupsInput input)
        {
            var query = Repository.GetAll().Include(at => at.Branch).AsQueryable();
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
            query = query.WhereIf(input.GroupType.HasValue, at => at.GroupType == input.GroupType);

            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var fuelGroups = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<FuelGroupDto>>(fuelGroups);
            return new PagedResultDto<FuelGroupDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<FuelGroupDto>> GetPaged(GetFuelGroupsPagedInput input)
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
                            FuelGroup fuelGroup = await Repository.FirstOrDefaultAsync(id);
                            if (fuelGroup != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _fuelGroupClinicRepository.CountAsync(a => a.FuelGroupId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.FuelGroups.HasClinics"));

                                    await Repository.DeleteAsync(fuelGroup);
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
                            FuelGroup fuelGroup = await Repository.FirstOrDefaultAsync(id);
                            if (fuelGroup != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _fuelGroupClinicRepository.CountAsync(a => a.FuelGroupId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.FuelGroups.HasClinics"));

                                    await Repository.DeleteAsync(fuelGroup);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll().Include(a => a.Branch).AsQueryable();

                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);

                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));

                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                    query = query.WhereIf(input.GroupType.HasValue, at => at.GroupType == input.GroupType);


                    int filteredCount = await query.CountAsync();
                    var fuelGroups = await query.Include(x => x.Branch)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<FuelGroupDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<FuelGroupDto>>(fuelGroups)
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
