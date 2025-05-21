using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.Contact.Dto;
using Sayarah.Application.DataTables.Dto;


namespace Sayarah.Application.Contact;

public interface IContactMessageAppService : IAsyncCrudAppService<ContactMessageDto, long, GetAllContactMessages, CreateContactMessageDto, ContactMessageDto>
{
    Task<DataTableOutputDto<ContactMessageDto>> GetPaged(GetContactMessagesInput input);
    Task<GetContactMessagesOutput> GetAllMessages(GetContactMessageByType input);
    Task<ContactMessageDto> SetAsSeen(EntityDto<long> input);
   

    #region NewsLetter
    Task<PagedResultDto<ContactMessageDto>> GetAllNewsLetter(GetAllContactMessages input);
    Task<bool> DeleteAllNewsLetter();
    Task<bool> DeleteSelected(GetAllContactMessages input);
    Task<bool> MakeAllAsRead(GetAllContactMessages input);
    Task<bool> SendToAll(GetAllContactMessages input);
    Task<bool> SendToSelected(GetAllContactMessages input);
    #endregion
}
