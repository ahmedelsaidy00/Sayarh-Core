using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;

namespace Sayarah.Application.Transactions.FuelTransactions
{
    public interface IFuelTransOutAppService : IAsyncCrudAppService<FuelTransOutDto, long, GetFuelTransOutsInput, CreateFuelTransOutDto, UpdateFuelTransOutDto>
    {
        Task<DataTableOutputDto<FuelTransOutDto>> GetPaged(GetFuelTransOutsPagedInput input);
        Task<FuelTransOutDto> UpdateTransaction(UpdateFuelTransOutDto input);
        Task<decimal> GetFuelPrice(GetFuelPriceInput input);

        Task<string> ExportExcel(RequestFuelExcelDtoInput input);
        Task<string> ExportProviderExcel(RequestFuelExcelDtoInput input);
        Task<string> ExportExcelForCompany(RequestFuelExcelDtoInput input);

        Task<string> ExportExcelAdmin(RequestFuelExcelDtoInput input);
        Task<GetFuelTransoutOutput> GetAllPaged(GetFuelTransInsInput input);


        Task<GetBranchConsumptionReportOutput> GetBranchConsumptionReport(GetFuelTransInsInput input);
        Task<string> ExportBranchConsumption(RequestFuelExcelDtoInput input);
        Task<bool> NotifyUsers(NotifyInputDto input);
        Task<bool> UpdateFuelandQuantityAsync(UpdateFuelAndQuantityDto input);
        Task<bool> CancelFuelTransOut(CancelFuelTransOutDto input);
        Task<GetProviderConsumptionReportOutput> GetProviderConsumptionReport(GetFuelTransInsInput input);
        Task<string> ExportProviderConsumption(RequestFuelExcelDtoInput input);
    }
}
