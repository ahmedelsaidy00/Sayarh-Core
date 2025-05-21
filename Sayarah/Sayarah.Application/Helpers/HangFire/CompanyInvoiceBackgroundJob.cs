using Abp.Dependency;
using Sayarah.Application.CompanyInvoices;
using System.Threading.Tasks;

namespace Sayarah.Application.Helpers.HangFire
{
    public class CompanyInvoiceBackgroundJob : ITransientDependency
    {
        private readonly ICompanyInvoiceJopAppService _companyInvoiceJopAppService;

        public CompanyInvoiceBackgroundJob(ICompanyInvoiceJopAppService companyInvoiceJopAppService)
        {
            _companyInvoiceJopAppService = companyInvoiceJopAppService;
        }

        public async Task ExecuteTask()
        {
            await _companyInvoiceJopAppService.CreateMonthlyCompanyInvoice();
        }
    }
}
