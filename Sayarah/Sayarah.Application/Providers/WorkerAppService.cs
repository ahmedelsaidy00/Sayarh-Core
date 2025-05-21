using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Users;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.Providers
{
    public class WorkerAppService : AsyncCrudAppService<Worker, WorkerDto, long, GetWorkersInput, CreateWorkerDto, UpdateWorkerDto>, IWorkerAppService
    {

        private readonly IUserAppService _userService;
        private readonly ICommonAppService _commonService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly RoleManager _roleManager;
        public UserManager UserManager { get; set; }


        public WorkerAppService(
            IRepository<Worker, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<Provider, long> providerRepository,
            ICommonAppService commonService,
            RoleManager roleManager
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _providerRepository = providerRepository;
            _commonService = commonService;
            _roleManager = roleManager;
        }

        [AbpAuthorize]
        public override async Task<WorkerDto> GetAsync(EntityDto<long> input)
        {
            var Worker = await Repository.GetAll()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(Worker);
        }

        [AbpAuthorize]
        public override async Task<WorkerDto> CreateAsync(CreateWorkerDto input)
        {
            try
            {
                //Check if worker exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));
                var user = new UserDto();

                if (input.User != null)
                {
                    input.User.Name = input.Name;
                    input.User.Surname = input.Name;
                    input.User.PhoneNumber = input.PhoneNumber;
                    input.User.Avatar = input.Avatar;
                    input.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                    input.User.UserType = UserTypes.Worker;
                    input.User.IsActive = input.User.IsActive;
                    input.User.EmailAddress = input.EmailAddress;
                    user = await _userService.CreateNewUser(input.User);
                }
                if (user.Id > 0)
                {
                    input.UserId = user.Id;
                    input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Workers", CodeField = "Code" });
                    var worker = ObjectMapper.Map<Worker>(input);
                    worker = await Repository.InsertAsync(worker);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    // update user with comapny id 
                    var _user = await _userRepository.FirstOrDefaultAsync(user.Id);
                    _user.WorkerId = worker.Id;
                    _user.ProviderId = worker.ProviderId;
                    _user.MainProviderId = input.MainProviderId;
                    await _userRepository.UpdateAsync(_user);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    if (worker.ProviderId.HasValue)
                        await UpdateWorkersCount(new EntityDto<long> { Id = worker.ProviderId.Value });

                    return MapToEntityDto(worker);
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.Workers.Error.CantCreateUser"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public override async Task<WorkerDto> UpdateAsync(UpdateWorkerDto input)
        {

            var worker = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);

            if (!string.IsNullOrEmpty(worker.Avatar) && (string.IsNullOrEmpty(input.Avatar) || !worker.Avatar.Equals(input.Avatar)))
                Utilities.DeleteImage(7, worker.Avatar, new string[] { "600x600_" });


            if (input.User != null)
            {
                worker.User.Name = input.Name;
                worker.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                worker.User.Surname = input.Name;
                worker.User.Avatar = input.Avatar;
                worker.User.PhoneNumber = input.PhoneNumber;

                worker.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                       worker.User.Password : _userManager.PasswordHasher.HashPassword(worker.User, input.User.Password);

                input.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                       worker.User.Password : _userManager.PasswordHasher.HashPassword(worker.User, input.User.Password);

                worker.User.EmailAddress = string.IsNullOrEmpty(input.EmailAddress) ? input.User.EmailAddress : input.EmailAddress;



                if (!string.IsNullOrEmpty(worker.User.UserName))
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == worker.User.UserName && x.Id != worker.UserId && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }


                if (!string.IsNullOrEmpty(worker.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == worker.User.EmailAddress && x.Id != worker.UserId && !string.IsNullOrEmpty(x.EmailAddress) && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }
                await _userManager.UpdateAsync(worker.User);




            }
            ObjectMapper.Map(input, worker);

            await Repository.UpdateAsync(worker);
            return MapToEntityDto(worker);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var worker = await Repository.GetAsync(input.Id);
            if (worker == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");

            long id = worker.ProviderId.Value;


            await Repository.DeleteAsync(worker);


            if (id > 0)
                await UpdateWorkersCount(new EntityDto<long> { Id = id });


        }

        [AbpAuthorize]
        public override async Task<PagedResultDto<WorkerDto>> GetAllAsync(GetWorkersInput input)
        {
            var query = Repository.GetAll().Include(at => at.User).AsQueryable();
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
            query = query.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
            query = query.WhereIf(input.MainProviderId.HasValue, a => a.Provider.MainProviderId == input.MainProviderId);

            if (input.IsEmployee.HasValue && input.IsEmployee == true)
            {
                if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                {
                    query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                }
                else
                    return new PagedResultDto<WorkerDto>
                    (
                       0, new List<WorkerDto>()
                    );
            }
            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }
            var workers = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<WorkerDto>>(workers);
            return new PagedResultDto<WorkerDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<WorkerDto>> GetPaged(GetWorkersPagedInput input)
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

                            Worker worker = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (worker != null)
                            {
                                if (input.action == "Delete")
                                {
                                    //// check before delete 
                                    //int clinicsCount = await _workerClinicRepository.CountAsync(a => a.WorkerId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Workers.HasClinics"));

                                    worker.User.IsDeleted = true;
                                    worker.User.DeletionTime = DateTime.Now;
                                    worker.User.DeleterUserId = AbpSession.UserId;
                                    worker.IsDeleted = true;
                                    worker.DeletionTime = DateTime.Now;
                                    worker.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(worker);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                //else if (input.action == "Restore")
                                //    worker.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(worker.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(worker);
                                }
                                else if (input.action == "Deactive")
                                {
                                    var user = await _userManager.FindByIdAsync(worker.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(worker);
                                }


                                else if (input.action == "Restore")
                                {
                                    worker.UnDelete();
                                    worker.User.UnDelete();
                                    // check if user role is deleted 

                                    string roleName = RolesNames.Worker;

                                    worker.User.Roles = new List<UserRole>();
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        worker.User.Roles.Add(new UserRole(AbpSession.TenantId, worker.UserId.Value, role.Id));

                                }


                                if (worker.ProviderId.HasValue)
                                    await UpdateWorkersCount(new EntityDto<long> { Id = worker.ProviderId.Value });
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt32(input.ids[0]);
                            Worker worker = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (worker != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _workerClinicRepository.CountAsync(a => a.WorkerId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Workers.HasClinics"));

                                    worker.User.IsDeleted = true;
                                    worker.User.DeletionTime = DateTime.Now;
                                    worker.User.DeleterUserId = AbpSession.UserId;
                                    worker.IsDeleted = true;
                                    worker.DeletionTime = DateTime.Now;
                                    worker.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(worker);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                //else if (input.action == "Restore")
                                //    worker.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(worker.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                }
                                else if (input.action == "Deactive")
                                {
                                    var user = await _userManager.FindByIdAsync(worker.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                }


                                else if (input.action == "Restore")
                                {
                                    worker.UnDelete();
                                    worker.User.UnDelete();
                                    // check if user role is deleted 

                                    string roleName = RolesNames.Worker;

                                    worker.User.Roles = new List<UserRole>();
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        worker.User.Roles.Add(new UserRole(AbpSession.TenantId, worker.UserId.Value, role.Id));

                                }



                                if (worker.ProviderId.HasValue)
                                    await UpdateWorkersCount(new EntityDto<long> { Id = worker.ProviderId.Value });
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                        .Include(a => a.User)
                        .Include(a => a.Provider)
                        .Where(at => at.User.UserType == UserTypes.Worker);
                    query = query.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, a => a.Provider.MainProviderId == input.MainProviderId);

                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                        }
                        else
                            return new DataTableOutputDto<WorkerDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<WorkerDto>()
                            };
                    }


                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.PhoneNumber.Contains(input.PhoneNumber));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.EmailAddress), at => at.EmailAddress.Contains(input.EmailAddress));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.User.UserName.Contains(input.UserName));

                    query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);
                    int filteredCount = await query.CountAsync();
                    var workers = await query.Include(x => x.User)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<WorkerDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<WorkerDto>>(workers)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<WorkerDto> GetByUserId(EntityDto<long> input)
        {
            var Worker = await Repository.GetAllIncluding(x => x.User)
                .Include(a => a.Provider.MainProvider.User)
                .Include(a => a.Provider.User)
                .FirstOrDefaultAsync(x => x.UserId == input.Id);
            return MapToEntityDto(Worker);
        }

        [AbpAuthorize]
        public async Task<WorkerDto> UpdateWorkerPhotoAsync(UpdateWorkerDto input)
        {
            var worker = await Repository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);

            if (!string.IsNullOrEmpty(worker.Avatar))
                Utilities.DeleteImage(7, worker.Avatar, new string[] { "600x600_" });

            worker.Avatar = input.Avatar;

            await Repository.UpdateAsync(worker);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            var user = await _userRepository.FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
            if (user != null)
            {
                user.Avatar = input.Avatar;
                await _userRepository.UpdateAsync(user);
                await UnitOfWorkManager.Current.SaveChangesAsync();
            }

            return MapToEntityDto(worker);
        }

        [AbpAuthorize]
        public async Task<WorkerDto> UpdateMobile(UpdateWorkerProfileInput input)
        {
            try
            {
                var worker = await Repository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);


                var user = await UserManager.GetUserByIdAsync(AbpSession.UserId.Value);
                user.Name = input.Name;
                user.Surname = input.Name;

                await UserManager.UpdateAsync(user);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                input.Id = worker.Id;
                input.Avatar = worker.Avatar;
                ObjectMapper.Map(input, worker);

                await Repository.UpdateAsync(worker);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                return MapToEntityDto(worker);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<bool> UpdateWorkersCount(EntityDto<long> input)
        {
            try
            {
                var provider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == input.Id);

                var workersCount = await Repository.CountAsync(a => a.ProviderId == input.Id && a.IsDeleted == false);
                provider.WorkersCount = workersCount;
                await _providerRepository.UpdateAsync(provider);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
