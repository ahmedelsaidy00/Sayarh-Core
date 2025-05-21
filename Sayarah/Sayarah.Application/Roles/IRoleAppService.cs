using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Collections.Generic;
using Sayarah.Application.Roles.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Roles
{
    public interface IRoleAppService : IAsyncCrudAppService<RoleDto, int, PagedResultRequestDto, CreateRoleDto, RoleDto>
    {
        Task<DataTableOutputDto<RoleDto>> GetPaged(GetRolesInput input);
        /////////////////////////////////////////Permissions Center///////////////////////////////////////////
        Task<ListResultDto<PermissionDto>> GetAllPermissions();
        IEnumerable<UserPermissionDto> GetAllParentPermissions();

        //////////Role Permissions//////////////
        Task<GetRoleWithPermissionsOutput> GetRoleWithPermissionsById(RolePermissionsInput input);
        Task SaveRolePermissions(SaveRolePermissionsInput input);
        Task<GetRoleWithPermissionsOutput> ClearRoleExplicitPermissions(RolePermissionsInput input);

        //////////User Permissions//////////////
        Task<GetUserWithPermissionsOutput> GetUserWithPermissionsById(UserPermissionsInput input);
        //Task<GetUserWithPermissionsOutput> GetPermissionsById(UserPermissionsInput input);
        Task SaveUserPermissions(SaveUsersPermissionsInput input);
        Task<GetUserWithPermissionsOutput> ClearUserExplicitPermissions(UserPermissionsInput input);


        Task<bool> CreatePermissions(CreatePermissionsInput input);



        Task<GetMainEmployeeWithPermissionsOutput> GetMainProviderEmployeePermissions(UserPermissionsInput input);
        Task SaveMainProviderUserPermissions(SaveUsersPermissionsInput input);


        Task<GetMainEmployeeWithPermissionsOutput> GetCompanyEmployeePermissions(UserPermissionsInput input);
        Task SaveCompanyUserPermissions(SaveUsersPermissionsInput input);
    }
}
