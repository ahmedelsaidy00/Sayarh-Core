using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Abp.Authorization.Users;
using Abp.Extensions;
using Sayarah.Core.Helpers;

namespace Sayarah.Web.Models.Account
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        public string Password { get; set; }

        public bool IsExternalLogin { get; set; }

        public string ExternalLoginAuthSchema { get; set; }
        public string PhoneNumber { get; set; }
        public long? NationalityId { get; set; }
        public long? SpecialtyId { get; set; }
        public string VideoFile { get; set; }
        public string Avatar { get; set; }
        public Gender? Gender { get; set; }
        public UserTypes? UserType { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!UserName.IsNullOrEmpty())
            {
                var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                if (!UserName.Equals(EmailAddress) && emailRegex.IsMatch(UserName))
                {
                    yield return new ValidationResult("Username cannot be an email address unless it's same with your email address !");
                }
            }
        }
    }

    public class StoreBranchItem
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string AddressOnMap { get; set; }

    }
}