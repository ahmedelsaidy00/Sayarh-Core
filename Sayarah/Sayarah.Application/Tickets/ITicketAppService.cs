using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using Sayarah.Application.Tickets.Dto;
using Sayarah.Tickets;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Tickets
{
    public interface ITicketAppService : IAsyncCrudAppService<TicketDto, long, GetAllTicket, CreateTicketDto, UpdateTicketDto>
    {
        Task<DataTableOutputDto<TicketDto>> GetPaged(GetTicketInput input);
        Task<TicketDto> CompleteTicket(Ticket ticket);

        //Task<TicketDetailDto> CreateTicketDetail(CreateTicketDetailDto input);
    }
}
