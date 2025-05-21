using Abp.Application.Services;
using Sayarah.Application.Tickets.Dto;

namespace Sayarah.Application.Tickets
{
    public interface ITicketDetailAppService : IAsyncCrudAppService<TicketDetailDto, long, GetAllTicketDetail, CreateTicketDetailDto, UpdateTicketDetailDto>
    {
        Task<TicketDetailDto> CreateTicketDetail(CreateTicketDetailDto input);
    }
}
