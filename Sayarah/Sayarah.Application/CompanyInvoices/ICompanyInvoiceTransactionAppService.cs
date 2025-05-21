using Abp.Application.Services;
using System.Threading.Tasks;
using Sayarah.Application.CompanyInvoices.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.CompanyInvoices
{
    public interface ICompanyInvoiceTransactionAppService : IAsyncCrudAppService<CompanyInvoiceTransactionDto , long, GetAllCompanyInvoiceTransactions , CreateCompanyInvoiceTransactionDto , UpdateCompanyInvoiceTransactionDto>
    {
        Task<DataTableOutputDto<CompanyInvoiceTransactionDto>> GetPaged(GetCompanyInvoiceTransactionsInput input);
    }
}
