using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Invoices.Dto;
using System.Threading.Tasks;

namespace Sayarah.Application.Invoices
{
    public interface IVoucherAppService : IAsyncCrudAppService<VoucherDto , long, GetAllVouchers , CreateVoucherDto , UpdateVoucherDto>
    {
        Task<DataTableOutputDto<VoucherDto>> GetPaged(GetVouchersInput input);
        Task<VoucherDto> CreateVoucher(CreateVoucherDto input);
        Task<VoucherDto> GetVoucherDetails(GetAllVouchers input);

        Task<GetTotalVouchersAmountOutput> GetTotalVouchersAmount(GetAllVouchers input);
    
    }
}
