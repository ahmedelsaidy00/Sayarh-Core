using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Authorization.Users;
using System.Collections.Generic;
using Sayarah.Application.Users.Dto;
using Sayarah.Application.Roles.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, GetAllUsersInput, CreateUserDto, UpdateUserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();
        Task<ChangeUserPasswordOutput> ChangeUserPassword(UpdateUserInput input);
        Task<List<User>> GetFilteredForRegister(GetPagedInput input);
        Task DeleteForRegister(long id);
        Task<UserDto> UpdateUserApi(CreateNewUserInput input);
        Task<List<User>> GetUsersByRole(EntityDto<int> input);
        //
        Task<string> UserManageDevices(ManageUserDevicesInput input);
        Task<long> UserGetByDevice(GetUserByDeviceInput input);
        Task<List<UserDevice>> GetAllUserDevices(GetAllUserDevicesInput input);
        Task<List<UserPlainDto>> GetAllUsersHaveDevices(GetAllUserDevicesInput input);
        Task<UserDto> GetById(EntityDto<long> input);
        Task SendNotifications(SendNotificationsInput input);
        Task<bool> SendNotificationToUsers(SendNotificationsInput input);
        Task SendNotificationsToAll(SendNotificationsInput input);
        Task<DataTableOutputDto<UserDto>> GetPaged(GetPagedInput input);
        Task<UserDto> CreateNewUser(CreateNewUserInput input);
        Task<UserDto> UpdateUser(CreateNewUserInput input);
        Task<string> GetAvatarPath(EntityDto<long> input);
        Task<UserDto> GetUserById(EntityDto<long> input);
        Task<User> GetUserByPhone(GetUserByPhone input);
        Task<DeactivateAccountOutput> DeactivateAccount(EntityDto<long> input);

        Task<UpdateMainColorDto> UpdateMainColor(UpdateMainColorDto input);
        Task<UpdateDarkModeDto> UpdateDarkMode(UpdateDarkModeDto input);

        Task<SendEmailCodeOutput> HandleEmailAddress(SendEmailCodeInput input);
        Task<HandleConfirmEmailAddressOutput> HandleConfirmEmailAddress(HandleConfirmEmailAddressInput input);


        Task<HandleConfirmEmailAddressOutput> CreatePassword(HandleConfirmEmailAddressInput input);

        Task<bool> UpdatePassword(UpdatePasswordInput input);
    }
}