using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Sayarah.Authorization.Users;
using System.ComponentModel.DataAnnotations;

namespace Sayarah.Application.Users.Dto
{
    [AutoMapTo(typeof(User))]
    public class UpdateUserDto: EntityDto<long>
    {
        public string Avatar { get; set; }
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        //[Required]
        //[StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        //[Required]
        //[EmailAddress]
        //[StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }


        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public bool AllBranches { get; set; }
    }


    public class UpdateMainColorDto : EntityDto<long>
    {
        public string MainColor { get; set; }
    }
    public class UpdateDarkModeDto : EntityDto<long>
    {
        public bool DarkMode { get; set; }
    }


    public class SendEmailCodeInput 
    {
        public string EmailAddress { get; set; }
    }

    public class SendEmailCodeOutput : EntityDto<long>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
       
        public string EmailAddress { get; set; }
        public string EmailConfirmationCode { get; set; }
    }


    public class HandleConfirmEmailAddressInput : EntityDto<long>
    {
        public string EmailAddress { get; set; }
        public string EmailConfirmationCode { get; set; }
        public string Password { get; set; }
    }


    public class HandleConfirmEmailAddressOutput : EntityDto<long>
    {
        public string EmailAddress { get; set; }
        public string EmailConfirmationCode { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
    }

}