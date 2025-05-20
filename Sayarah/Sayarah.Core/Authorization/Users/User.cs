using Abp.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using Sayarah.Companies;
using Sayarah.Helpers.Enums;
using Sayarah.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe";

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16);
        }
        public virtual string Code { get; set; }
        public virtual string Avatar { get; set; }
        public virtual UserTypes UserType { get; set; }
        public virtual long? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public virtual long? WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        public virtual Worker Worker { get; set; }


        public virtual long? MainProviderId { get; set; }
        [ForeignKey("MainProviderId")]
        public virtual MainProvider MainProvider { get; set; }


        public virtual List<UserDevice> UserDevices { get; set; }
        public virtual List<UserDashboard> UserDashboards { get; set; }



        public virtual string MainColor { get; set; }
        public virtual bool DarkMode { get; set; }
        public virtual bool AllBranches { get; set; }


        public virtual void SetEmailConfirmationCode()
        {
            IsEmailConfirmed = false;

            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("/", "");
            GuidString = GuidString.Replace("+", "");
            GuidString = GuidString.Replace("=", "");
            EmailConfirmationCode = GuidString;
        }
        public virtual void SetCodeConfirmed()
        {
            IsEmailConfirmed = true;
            EmailConfirmationCode = string.Empty;
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress, string password)
        {
            var hasher = new PasswordHasher<User>();

            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
            };

            user.Password = hasher.HashPassword(user, password);

            user.SetNormalizedNames();

            return user;
        }
    }
}