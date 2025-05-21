using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Invoices.Dto;
using System.Threading.Tasks;
using static Sayarah.Application.Invoices.InvoiceAppService;

namespace Sayarah.Application.Invoices
{
    public interface IInvoiceAppService : IAsyncCrudAppService<InvoiceDto , long, GetAllInvoices , CreateInvoiceDto , UpdateInvoiceDto>
    {
        Task<DataTableOutputDto<InvoiceDto>> GetPaged(GetInvoicesInput input);
        Task<InvoiceDto> CreateInvoice(CreateInvoiceInput input);
        Task<InvoiceDto> GetInVoiceDetails(GetAllInvoices input);

        Task<InvoiceOutput> PrintInvoiceDetails(GetAllInvoices input);
    }
}
