using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Sayarah.Authorization.Users;
using static Sayarah.SayarahConsts;
using System.Collections.Generic;
using Sayarah.Application.Helpers;
using Sayarah.Core.Helpers;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }
        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }
        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreationTime { get; set; }
        //public string[] Roles { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public string Avatar { get; set; }
        public string AvatarPath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && (Avatar.Contains("https://platform-lookaside.fbsbx.com/") || Avatar.Contains("https://lh3.googleusercontent.com/")))
                {
                    return Avatar;
                }

                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(1, "400x400_" + Avatar))
                {
                    return FilesPath.Users.ServerImagePath + "400x400_" + Avatar;
                }
                else
                {
                    return FilesPath.Users.DefaultImagePath;
                }

            }
        }
        public UserTypes UserType { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? WorkerId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public bool IsSpecial { get; set; }

        public List<UserDashboardDto> UserDashboards { get; set; }
        public bool AllBranches { get; set; }
    }

    [AutoMapFrom(typeof(User))]
    public class UserPlainDto : EntityDto<long>
    {
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; set; }
        public UserTypes UserType { get; set; }
        public string Code { get; set; }
        public bool AllBranches { get; set; }

    }

    [AutoMapFrom(typeof(User))]
    public class UserApiDto : EntityDto<long>
    {
        public string Avatar { get; set; }
        public string AvatarPath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && (Avatar.Contains("https://platform-lookaside.fbsbx.com/") || Avatar.Contains("https://lh3.googleusercontent.com/")))
                    return Avatar;
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(1, "400x400_" + Avatar))
                    return FilesPath.Users.ServerImagePath + "400x400_" + Avatar;
                else
                    return FilesPath.Users.DefaultImagePath;
            }
        }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public UserTypes UserType { get; set; }
        public string Code { get; set; }
        public bool AllBranches { get; set; }
    }

    [AutoMapFrom(typeof(User))]
    public class ShortUserDto : EntityDto<long>
    {
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
         
    }

    [AutoMapFrom(typeof(User))]
    public class SmallUserDto : EntityDto<long>
    {
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
         
    }
    public class GetPagedInput : DataTableInputDto
    {
        public long? IsSpecialId { get; set; } 
        public string RoleName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public UserTypes? UserType { get; set; }
        public string Code { get; set; }
        public bool? IsActive { get; set; }

        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? CurrentEmployeeId { get; set; }

        public bool? MainEmployees { get; set; }

    }
    [AutoMapTo(typeof(User))]
    public class CreateNewUserInput : EntityDto<long>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string RoleName { get; set; }
        public bool? IsActive { get; set; }
        public string Avatar { get; set; }
        public UserTypes UserType { get; set; }
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }

        public bool AllBranches { get; set; }
        public List<ManageUserDashboardDto> UserDashboards { get; set; }

    }
 


    public class ManageUserDevicesInput
    {
        public long? UserId { get; set; }
        public string RegistrationToken { get; set; }
        public DeviceType DeviceType { get; set; }

    }
    public class GetUserByDeviceInput
    {
        public string RegistrationToken { get; set; }
        public DeviceType DeviceType { get; set; }
    }

    public class GetAllUserDevicesInput
    {
        public string UserName { get; set; }
    }
    public class SendNotificationsInput
    {
        public long[] Ids { get; set; }
        public List<NotificationUserObject> UsersList { get; set; }
        public string NotificationMessage { get; set; }
        public NotificationFilter? NotificationFilter { get; set; }
        public NotificationType? NotificationType { get; set; }
    }

    public class NotificationUserObject : EntityDto<long>
    {
        public string EmailAddress { get; set; }
    }

    public class UpdateUserInput : EntityDto<long>
    {

        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

    }
    public class ChangeUserPasswordOutput
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class GetAllUsersInput : PagedResultRequestDto
    {
        public string Name { get; set; }
        public UserTypes? UserType { get; set; }
        public string PhoneNumber { get; set; }
        public bool MaxCount { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string Code { get; set; }
        public long? CurrentUserId { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
    }
    public class GetUserByPhone
    {
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public UserTypes? UserType { get; set; }
    }
    public class ForgotPasswordInput
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
    public class NotifyUserforPackageExpireDateInput
    {
        public long Id { get; set; }
    }
    public class DeactivateAccountOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ApiUserDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string AvatarPath { get; set; }
    }


    public class UpdatePasswordInput : EntityDto<long>
    {
        public string Password { get; set; }
    }

}