using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Localization;

namespace Sayarah.Application.Roles.Dto
{
    [AutoMapFrom(typeof(Permission))]
    public class PermissionDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }
    }
    [AutoMapFrom(typeof(Permission))]
    public class UserPermissionDto : EntityDto
    {
        public string Name { get; set; }
        public ILocalizableString DisplayName { get; set; }
        public ILocalizableString Description { get; set; }
        public bool IsGrantedByDefault { get; set; }
    }


}