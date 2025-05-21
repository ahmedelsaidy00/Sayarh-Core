using Abp.Application.Services;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using System.Collections.Generic;
using Sayarah.Application.Helpers.StoredProcedures.Dto;

namespace Sayarah.Application.Helpers.StoredProcedures
{
    public interface IStoredProcedureAppService : IApplicationService
	{
        Task<GetTransactionsReportOutput> GetTransactionsReport(GetTransactionsReportInput input);
        Task<GetTransactionsReportPagedOutput> GetTransactionsReportPaged(GetTransactionsReportInput input);
        Task<GetAdminDashboardStatisticsOutput> GetAdminDashboardStatistics(GetDashboardStatisticsInput input);
        Task<GetCompanyDashboardStatisticsOutput> GetCompanyDashboardStatistics(GetDashboardStatisticsInput input);
        Task<GetBranchDashboardStatisticsOutput> GetBranchDashboardStatistics(GetDashboardStatisticsInput input);

        Task<GetProviderDashboardStatisticsOutput> GetProviderDashboardStatistics(GetDashboardStatisticsInput input);

        Task<GetInvoicesReportOutput> GetInvoicesReport(GetTransactionsReportInput input);
        Task<GetVouchersReportOutput> GetVouchersReport(GetTransactionsReportInput input);

        Task UpdateInvoiceCodeInFuelTransOuts(UpdateInvoiceCodeInFuelTransOutsInput input);
        Task UpdateCompanyInvoiceCodeInFuelTransOuts(UpdateInvoiceCodeInFuelTransOutsInput input);
        Task<GetProviderStatmentReportOutput> GetProviderStatmentReport(GetTransactionsReportInput input);

        Task<string> ExportProviderStatment(ExportProviderStatmentInput input);

        Task<GetMonthlyFuelStatsByTypeOutput> GetMonthlyFuelStatsByType(GetDashboardStatisticsInput input);

        Task<GetFuelTypesStatisticsOutput> GetFuelTypesStatistics(GetDashboardStatisticsInput input);
        Task CopySubscriptionToTransactions(CopySubscriptionToTransactionsInput input);

        Task<GetProviderRevenueOutput> GetProviderRevenue(GetDashboardStatisticsInput input);
        Task<string> ExportProviderRevenue(GetProviderRevenueExcelDtoInput input);

        Task<GetCompanyStatmentReportOutput> GetCompanyStatmentReport(GetTransactionsReportInput input);
        Task<string> ExportCompanyStatment(ExportProviderStatmentInput input);
        Task<GetMonthlyFuelConsumptionReportOutput> GetMonthlyFuelConsumptionReport(GetMonthlyFuelConsumptionReportInput input);
        Task<List<WalletsPendingTransOutput>> WalletsPendingTrans(WalletsPendingTrans input);

        //Task<GetBranchConsumptionReportOutput> GetBranchConsumptionReport(GetTransactionsReportInput input);

    }
}
