using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
namespace Sayarah.Core.Repositories
{
    public interface ICommonRepository
    {
        int ExecuteSql(string sql, params object[] parameters);
        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);
        Task<List<T>> ExecuteSql2Async<T>(string sql) where T : class;
        Task ExecuteStoredProcedureAsync(string storedProcedureName, List<SqlParameter> storedProcedureParameter);
        Task<List<T>> GetListAsync<T>(string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class;
        Task<List<T>> GetListAsync<T>(string sql) where T : class;
        Task<(List<T> Items, int TotalCount)> GetListWithCountAsync<T>(
       string storedProcedureName,
       List<SqlParameter> storedProcedureParameters) where T : class, new();

        Task<T> GetAsync<T>(string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class;
        Task<T> GetAsync<T>(string sql) where T : class;
        Task<int> ExecuteSqlAsync(string sql);

        Task<bool> CheckRelatedTable(string TableName, long ID, string ExceptTableName);
        Task<string> GetNextCode(string tableName, string codeField, string addWhere);
        //SitePages
        Task<string> GetNextKey(int pageSectionId, int pageCategoryId);
        //
        DataSet GetDataSet(string storedProcedureName, List<SqlParameter> storedProcedureParameter);
        DataSet GetDataSet(string sql);
    }
}
