using Abp.Application.Services;
using Sayarah.Application.CompanyInvoices.Dto;

namespace Sayarah.Application.CompanyInvoices
{
    public interface ICompanyInvoiceJopAppService : IAsyncCrudAppService<CompanyInvoiceDto , long, GetAllCompanyInvoices , CreateCompanyInvoiceDto , UpdateCompanyInvoiceDto>
    {
        Task<List<CompanyInvoiceOutput>> PrintCompanyInvoiceDetails(GetAllCompanyInvoices input);

        Task<CompanyInvoiceDto> CreateMonthlyCompanyInvoice();
    }
}
