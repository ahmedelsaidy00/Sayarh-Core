using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Invoices.Dto;
using System.Threading.Tasks;

namespace Sayarah.Application.Invoices
{
    public interface IInvoiceTransactionAppService : IAsyncCrudAppService<InvoiceTransactionDto , long, GetAllInvoiceTransactions , CreateInvoiceTransactionDto , UpdateInvoiceTransactionDto>
    {
        Task<DataTableOutputDto<InvoiceTransactionDto>> GetPaged(GetInvoiceTransactionsInput input);
    }
}
