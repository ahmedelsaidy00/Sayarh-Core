using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.RegisterationRequests.Dto;
using Sayarah.RegisterationRequests;

namespace Sayarah.Application.RegisterationRequests
{
    public interface IRegisterationRequestAppService : IAsyncCrudAppService<RegisterationRequestDto, long, GetRegisterationRequestsInput, CreateRegisterationRequestDto, UpdateRegisterationRequestDto>
    {
        Task<DataTableOutputDto<RegisterationRequestDto>> GetPaged(GetRegisterationRequestsPagedInput input);
        Task<RegisterationRequestDto> ManageRequest(UpdateRegisterationRequestDto input);

        Task<RegisterationRequestDto> HandlePhoneNumber(UpdateRegisterationRequestDto input);
        Task<RegisterationRequestDto> HandleConfirmPhoneNumber(UpdateRegisterationRequestDto input);

        Task<RegisterationRequestDto> HandleEmailAddress(UpdateRegisterationRequestDto input);
        Task<RegisterationRequestDto> HandleConfirmEmailAddress(UpdateRegisterationRequestDto input);

        Task<RegisterationRequestDto> CompleteRegisteration(UpdateRegisterationRequestDto input);
        Task<RegisterationRequestDto> RefuseRegisterationRequest(UpdateRegisterationRequestDto input);
        Task<RegisterationRequestDto> ApproveRegisterationRequest(EntityDto<long> input);


        Task<bool> TestSendNotification(RegisterationRequest input);
    }
}
