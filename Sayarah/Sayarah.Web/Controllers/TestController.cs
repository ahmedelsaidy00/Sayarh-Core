using System;
using System.Threading.Tasks;
using System.Web.Http;
using Sayarah.Application.CompanyInvoices;
using Sayarah.CompanyInvoices;

namespace Sayarah.Web.Controllers
{

  
    public class TestController : ApiController
    {
        private readonly ICompanyInvoiceJopAppService _companyInvoiceJopAppService;

        public TestController(ICompanyInvoiceJopAppService companyInvoiceJopAppService)
        {
            _companyInvoiceJopAppService = companyInvoiceJopAppService;
        }
        //public async Task<System.Web.Http.Results.OkNegotiatedContentResult<string>> Test()
        //{
        //    Console.WriteLine("callll");
        //    await _companyInvoiceJopAppService.CreateMonthlyCompanyInvoice();
        //    return Ok("dd");
        //}
    }


}