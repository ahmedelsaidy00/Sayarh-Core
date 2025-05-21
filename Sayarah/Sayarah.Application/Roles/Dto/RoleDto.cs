using Abp.Application.Services.Dto;
using Abp.Authorization.Roles;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Roles;
using System.ComponentModel.DataAnnotations;

namespace Sayarah.Application.Roles.Dto
{
    [AutoMapFrom(typeof(Role)), AutoMapTo(typeof(Role))]
    public class RoleDto : EntityDto<int>
    {
        [Required]
        [StringLength(AbpRoleBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpRoleBase.MaxDisplayNameLength)]
        public string DisplayName { get; set; }

        [StringLength(Role.MaxDescriptionLength)]
        public string Description { get; set; }

        public bool IsStatic { get; set; }

        public List<string> Permissions { get; set; }
    }

    public class GetRolesInput : DataTableInputDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool MatchExact { get; set; }

    }
    public class RolePermissionsInput
    {
        [Required]
        public int RoleId { get; set; }
        public string ParentName { get; set; }
    }
    public class UserPermissionsInput
    {
        [Required]
        public long UserId { get; set; }
        public long? MainProviderId { get; set; }
        public long? CompanyId { get; set; }
        public string ParentName { get; set; }
       
    }


   

    public class CustomPermissionDto : BaseInputDto
    {
        public string Name { get; set; }
    }

    public class GetRoleWithPermissionsOutput
    {
        public RoleDto Role { get; set; }
        public IReadOnlyList<UserPermissionDto> GrantedPermissions { get; set; }
        public IEnumerable<UserPermissionDto> AllPermissions { get; set; }
    }

    public class SaveRolePermissionsInput
    {
        public int RoleId { get; set; }
        public List<CustomPermissionDto> Permissions { get; set; }
    }



    public class GetUserWithPermissionsOutput
    {
        public UserDto User { get; set; }
        public IReadOnlyList<UserPermissionDto> GrantedPermissions { get; set; }
        public IEnumerable<UserPermissionDto> AllPermissions { get; set; }
    }

    public class GetMainEmployeeWithPermissionsOutput
    {
        public UserDto User { get; set; }
        public IReadOnlyList<UserPermissionDto> GrantedPermissions { get; set; }
        public IReadOnlyList<UserPermissionDto> MainGrantedPermissions { get; set; }
        public IEnumerable<UserPermissionDto> AllPermissions { get; set; }
    }


    public class SaveUsersPermissionsInput
    {
        public long UserId { get; set; }
        public List<CustomPermissionDto> Permissions { get; set; }
    }


    public class CreatePermissionsInput
    {
        public long Id { get; set; }
        public string ParentName { get; set; }
    }

}
