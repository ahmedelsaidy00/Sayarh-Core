using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.MultiTenancy;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sayarah.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        public LogInManager(
            AbpUserManager<Role, User> userManager,
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IRepository<UserLoginAttempt, long> userLoginAttemptRepository,
            IUserManagementConfig userManagementConfig, IIocResolver iocResolver,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher,
            AbpUserClaimsPrincipalFactory<User, Role> abpUserClaimsPrincipalFactory)
            : base(userManager, multiTenancyConfig, tenantRepository, unitOfWorkManager, settingManager, userLoginAttemptRepository, userManagementConfig, iocResolver, passwordHasher, roleManager, abpUserClaimsPrincipalFactory)
        {
        }
        public virtual async Task<AbpLoginResult<Tenant, User>> LoginByUsernameAsync(string userName, string plainPassword, string tenancyName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));

            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentNullException(nameof(plainPassword));

            // Resolve tenant
            Tenant tenant = null;
            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                if (!MultiTenancyConfig.IsEnabled)
                {
                    tenant = await GetDefaultTenantAsync();
                }
                else if (!string.IsNullOrWhiteSpace(tenancyName))
                {
                    tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                    if (tenant == null)
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);

                    if (!tenant.IsActive)
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                }
            }

            var tenantId = tenant?.Id;

            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                var user = UserManager.Users.FirstOrDefault(x => x.UserName == userName);
                if (user == null)
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);

                if (await UserManager.IsLockedOutAsync(user))
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);

                // Example: apply tenant-specific lockout settings (you'd store these in the DB)
                //if (tenantId.HasValue)
                //{
                //    var tenantSettings = await GetTenantSecuritySettingsAsync(tenantId.Value);
                //    UserManager.Options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(tenantSettings.LockoutDurationMinutes);
                //    UserManager.Options.Lockout.MaxFailedAccessAttempts = tenantSettings.MaxFailedAttempts;
                //}

                var passwordResult = UserManager.PasswordHasher.VerifyHashedPassword(user, user.Password, plainPassword);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    await UserManager.AccessFailedAsync(user);

                    if (await UserManager.IsLockedOutAsync(user))
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);

                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidPassword, tenant, user);
                }

                if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
                    return await CreateLoginResultAsync(user, tenant); // optionally rehash and save password here

                await UserManager.ResetAccessFailedCountAsync(user);

                return await CreateLoginResultAsync(user, tenant);
            }
        }





        public virtual async Task<AbpLoginResult<Tenant, User>> LoginByUsernameOrEmail(string userName, string plainPassword, string tenancyName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (string.IsNullOrEmpty(plainPassword))
            {
                throw new ArgumentNullException(nameof(plainPassword));
            }

            //Get and check tenant
            Tenant tenant = null;
            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                if (!MultiTenancyConfig.IsEnabled)
                {
                    tenant = await GetDefaultTenantAsync();
                }
                else if (!string.IsNullOrWhiteSpace(tenancyName))
                {
                    tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                    if (tenant == null)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                    }

                    if (!tenant.IsActive)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                    }
                }
            }

            var tenantId = tenant == null ? (int?)null : tenant.Id;
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {

                var user = new User();

                if (userName.Contains("@") == true)
                    user = UserManager.Users.FirstOrDefault(x => x.EmailAddress == userName);
                else
                    user = UserManager.Users.FirstOrDefault(x => x.UserName == userName);

                if (user == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                }

                //UserManager.InitializeLockoutSettings(tenantId);
                var verificationResult = UserManager.PasswordHasher.VerifyHashedPassword(user, user.Password, plainPassword);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);

                }

                if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password = UserManager.PasswordHasher.HashPassword(user, plainPassword);
                    await UserManager.UpdateAsync(user);

                    // Return a result indicating the password was successfully rehashed
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.Success, tenant, user);
                }

                await UserManager.ResetAccessFailedCountAsync(user);


                return await CreateLoginResultAsync(user, tenant);
            }
        }



        public virtual async Task<AbpLoginResult<Tenant, User>> LoginByPhoneAsync(string phoneNumber, string plainPassword, string tenancyName)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            if (string.IsNullOrEmpty(plainPassword))
            {
                throw new ArgumentNullException(nameof(plainPassword));
            }

            //Get and check tenant
            Tenant tenant = null;
            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                if (!MultiTenancyConfig.IsEnabled)
                {
                    tenant = await GetDefaultTenantAsync();
                }
                else if (!string.IsNullOrWhiteSpace(tenancyName))
                {
                    tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                    if (tenant == null)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                    }

                    if (!tenant.IsActive)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                    }
                }
            }

            var tenantId = tenant == null ? (int?)null : tenant.Id;
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                var user = UserManager.Users.FirstOrDefault(x => x.PhoneNumber == phoneNumber && x.IsDeleted == false);
                if (user == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                }

                //UserManager.InitializeLockoutSettings(tenantId);
                var verificationResult = UserManager.PasswordHasher.VerifyHashedPassword(user, user.Password, plainPassword);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                }

                if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password = UserManager.PasswordHasher.HashPassword(user, plainPassword);
                    await UserManager.UpdateAsync(user);

                    // Return a result indicating the password was successfully rehashed
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.Success, tenant, user);
                }

                await UserManager.ResetAccessFailedCountAsync(user);

                if (user.IsPhoneNumberConfirmed == false)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.UserPhoneNumberIsNotConfirmed, tenant, user);
                }

                return await CreateLoginResultAsync(user, tenant);
            }
        }



        public virtual async Task<AbpLoginResult<Tenant, User>> LoginByPhoneFront(string phoneNumber, string tenancyName)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }



            //Get and check tenant
            Tenant tenant = null;
            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                if (!MultiTenancyConfig.IsEnabled)
                {
                    tenant = await GetDefaultTenantAsync();
                }
                else if (!string.IsNullOrWhiteSpace(tenancyName))
                {
                    tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                    if (tenant == null)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                    }

                    if (!tenant.IsActive)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                    }
                }
            }

            var tenantId = tenant == null ? (int?)null : tenant.Id;
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                var user = UserManager.Users.FirstOrDefault(x => x.PhoneNumber == phoneNumber && x.IsDeleted == false);
                if (user == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                }

                //UserManager.InitializeLockoutSettings(tenantId);

                await UserManager.ResetAccessFailedCountAsync(user);

                if (user.IsPhoneNumberConfirmed == false)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.UserPhoneNumberIsNotConfirmed, tenant, user);
                }

                return await CreateLoginResultAsync(user, tenant);
            }
        }
    }
}
