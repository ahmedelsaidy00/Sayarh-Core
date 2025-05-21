using Abp.Application.Services;
using Microsoft.Data.SqlClient;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Sayarah.Application.Helpers
{
    public interface ICommonAppService : IApplicationService
    {
        Task<string> GetNextCode(GetNextCodeInputDto input);
        Task<int> ExecuteSqlAsync(string _apikey, string _addApikey, string sql);
        Task ExecuteStoredProcedureAsync(string _apikey, string _addApikey, string storedProcedureName, List<SqlParameter> storedProcedureParameter);
        Task<List<T>> GetListAsync<T>(string _apikey, string _addApikey, string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class;
        Task<T> GetAsync<T>(string _apikey, string _addApikey, string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class;
        // Task DBBackup();
        Task<bool> CheckRelatedTable(string TableName, long ID, string ExceptTableName);
        //Task<GetDashboardStatisticsOutput> GetNumbers(GetDashboardStatisticsInput input);
        Task<string> GetNextKey(int pageSectionId, int pageCategoryId);
        Task<bool> SendEmail(SendEmailRequest input);
        Task<BestActiveProducts> GetBestSellerProducts();
        Task<BestActives> GetBestSellerUsers();
        Task<bool> HandleSiteMap();
        Task<List<FrequencyOutput>> GetFrequencyStations(GetFrequencyStationInput input);
        Task<string> FrequencyExportExcelAdmin(GetFrequencyStationInput input);
        Task<List<ConsumptionOutput>> GetConsumptions(GetConsumptionInput input);
        Task<string> ConsumptionExportExcelAdmin(GetConsumptionInput input);
        Task<List<VeichleConsumptionOutput>> GetVeichleConsumptions(GetVeichleConsumptionInput input);
        Task<string> VeichleConsumptionExportExcelAdmin(GetVeichleConsumptionInput input);
    }

}
