using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using Sayarah.Application.CompanyInvoices.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.CompanyInvoices
{
    public interface ICompanyInvoiceAppService : IAsyncCrudAppService<CompanyInvoiceDto , long, GetAllCompanyInvoices , CreateCompanyInvoiceDto , UpdateCompanyInvoiceDto>
    {
        Task<DataTableOutputDto<CompanyInvoiceDto>> GetPaged(GetCompanyInvoicesInput input);
        Task<DataTableOutputDto<CompanyInvoiceDto>> GetPagedMainProvides(GetCompanyInvoicesInput input);
        Task<DataTableOutputDto<CompanyInvoiceDto>> GetPagedAdmins(GetCompanyInvoicesInput input);
        Task<CompanyInvoiceDto> CreateCompanyInvoice(CreateCompanyInvoiceInput input);
        Task<CompanyInvoiceDto> GetInVoiceDetails(GetAllCompanyInvoices input);
        //Task<CompanyInvoiceOutput> PrintCompanyInvoiceDetails(GetAllCompanyInvoices input);

        //Task<CompanyInvoiceDto> CreateMonthlyCompanyInvoice();
    }
}
