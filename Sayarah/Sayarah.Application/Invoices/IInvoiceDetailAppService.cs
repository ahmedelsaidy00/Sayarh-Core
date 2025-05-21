using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Invoices.Dto;
using System.Threading.Tasks;

namespace Sayarah.Application.Invoices
{
    public interface IInvoiceDetailAppService : IAsyncCrudAppService<InvoiceDetailDto , long, GetAllInvoiceDetails , CreateInvoiceDetailDto , UpdateInvoiceDetailDto>
    {
        Task<DataTableOutputDto<InvoiceDetailDto>> GetPaged(GetInvoiceDetailsInput input);
    }
}
