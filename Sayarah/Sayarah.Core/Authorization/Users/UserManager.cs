using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Localization;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sayarah.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;


namespace Sayarah.Authorization.Users
{
    public class UserManager : AbpUserManager<Role, User>
    {
        private readonly IRepository<UserDevice, long> _userDeviceRepository;

        public UserManager(
            UserStore userStore,
            RoleManager roleManager,
            IPermissionManager permissionManager,
            IUnitOfWorkManager unitOfWorkManager,
            ICacheManager cacheManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IOrganizationUnitSettings organizationUnitSettings,
            ILocalizationManager localizationManager,
            ISettingManager settingManager,
            IRepository<UserLogin, long> userLogin,
            IRepository<UserDevice, long> userDeviceRepository,
            IOptions<IdentityOptions> options,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager> logger)
            : base(
                roleManager,
                userStore,
                options,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                services,
                logger,
                permissionManager,
                unitOfWorkManager,
                cacheManager,
                organizationUnitRepository,
                userOrganizationUnitRepository,
                organizationUnitSettings,
                settingManager,
                userLogin)
        {
            _userDeviceRepository = userDeviceRepository;
        }

        public async Task<List<UserDevice>> GetRegistrationTokens(long userId)
        {
            return await _userDeviceRepository.GetAllListAsync(m => m.UserId == userId);
        }

        public new async Task<Microsoft.AspNetCore.Identity.IdentityResult> CreateAsync(User user)
        {
            var result = await CheckDuplicateUsernameOrEmailAddressAsync(user.Id, user.UserName, user.EmailAddress);
            if (!result.Succeeded) return result;

            if (AbpSession.TenantId.HasValue && !user.TenantId.HasValue)
            {
                user.TenantId = AbpSession.TenantId;
            }


            return await base.CreateAsync(user);
        }

        public new async Task<IdentityResult> UpdateAsync(User user)
        {
            var result = await CheckDuplicateUsernameOrEmailAddressAsync(user.Id, user.UserName, user.EmailAddress);
            if (!result.Succeeded) return result;

            if (AbpSession.TenantId.HasValue && !user.TenantId.HasValue)
            {
                user.TenantId = AbpSession.TenantId;
            }


            return await base.UpdateAsync(user);
        }
    }

}