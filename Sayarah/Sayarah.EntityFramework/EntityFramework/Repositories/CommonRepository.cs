using Abp.Dependency;
using Abp.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sayarah.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Sayarah.EntityFramework.Repositories
{
    public class CommonRepository : ICommonRepository, ITransientDependency
    {

        private readonly IDbContextProvider<SayarahDbContext> _dbContextProvider;

        public CommonRepository(IDbContextProvider<SayarahDbContext> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        /// <summary>
        /// execute sql query with parameters
        /// </summary>
        /// <param name="sql">sql statment</param>
        /// <param name="parameters">sql statment's parameters</param>
        /// <returns></returns>
        public int ExecuteSql(string sql, params object[] parameters)
        { // Get the DbContext from the DbContextProvider
            var dbContext = _dbContextProvider.GetDbContext();

            // Execute the raw SQL query
            return dbContext.Database.ExecuteSqlRaw(sql, parameters);
        }

        /// <summary>
        /// execute sql query with parameters async
        /// </summary>
        /// <param name="sql">sql statment</param>
        /// <param name="parameters">sql statment's parameters</param>
        /// <returns></returns>
        public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            // Get the DbContext from the DbContextProvider
            var dbContext = _dbContextProvider.GetDbContext();

            // Execute the raw SQL query asynchronously
            return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }



        public async Task<int> ExecuteSqlAsync(string sql)
        {
            // Get the DbContext from the DbContextProvider
            var dbContext = _dbContextProvider.GetDbContext();

            // Execute the raw SQL query asynchronously
            return await dbContext.Database.ExecuteSqlRawAsync(sql);
        }


        /// <summary>
        /// execute storedProcedure with parameters async
        /// </summary>
        /// <param name="sql">storedProcedure name</param>
        /// <param name="parameters">storedProcedure's parameters</param>
        /// <returns></returns>
        public async Task ExecuteStoredProcedureAsync(string storedProcedureName, List<SqlParameter> storedProcedureParameter)
        {
            string parameterNames = string.Empty;

            for (int i = 0; i < storedProcedureParameter.Count; i++)
            {
                SqlParameter sqlParameter = storedProcedureParameter[i];
                if (i > 0)
                    parameterNames += ",";
                parameterNames += sqlParameter.ParameterName;
            }
            try
            {
                var dbContext = _dbContextProvider.GetDbContext();
                var query = string.Format("{0} {1}", storedProcedureName, parameterNames);

                // Execute the stored procedure and retrieve the result
                await dbContext.Set<object>()
                   .FromSqlRaw(query, storedProcedureParameter.ToArray())
                   .ToListAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<T>> GetListAsync<T>(string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class
        {
            try
            {
                string parameterNames = string.Empty;

                for (int i = 0; i < storedProcedureParameter.Count; i++)
                {
                    SqlParameter sqlParameter = storedProcedureParameter[i];
                    if (i > 0)
                        parameterNames += ",";
                    parameterNames += sqlParameter.ParameterName;
                }

                var dbContext = _dbContextProvider.GetDbContext();
                var query = string.Format("{0} {1}", storedProcedureName, parameterNames);

                // Execute the stored procedure and retrieve the result
                return await dbContext.Set<T>()
                    .FromSqlRaw(query, storedProcedureParameter.ToArray())
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //List<T> item = null;
            //item = await _dbContextProvider.GetDbContext().Set<T>().ToListAsync();
            //return item;
        }
        public async Task<(List<T> Items, int TotalCount)> GetListWithCountAsync<T>(
        string storedProcedureName,
        List<SqlParameter> parameters) where T : class, new()
        {
            var dbContext = _dbContextProvider.GetDbContext();
            var connection = dbContext.Database.GetDbConnection();


            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var items = new List<T>();

                    // Use DataTable to manually map results to T
                    var dataTable = new DataTable();
                    dataTable.Load(reader);

                    foreach (DataRow row in dataTable.Rows)
                    {
                        T obj = new T();
                        foreach (var prop in typeof(T).GetProperties())
                        {
                            if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                            {
                                prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                            }
                        }
                        items.Add(obj);
                    }

                    int totalCount = 0;

                    // Move to next result set for count
                    if (await reader.NextResultAsync())
                    {
                        if (await reader.ReadAsync())
                            totalCount = reader.GetInt32(0);
                    }

                    return (items, totalCount);
                }
            }
        }






        public async Task<List<T>> GetListAsync<T>(string sql) where T : class
        {
            try
            {
                return await _dbContextProvider.GetDbContext()
                                               .Set<T>() // Get the DbSet for type T
                                               .FromSqlRaw(sql) // Execute the SQL query
                                               .ToListAsync(); // Get the results as a list
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<T> GetAsync<T>(string storedProcedureName, List<SqlParameter> storedProcedureParameter) where T : class
        {
            try
            {
                string parameterNames = string.Empty;

                for (int i = 0; i < storedProcedureParameter.Count; i++)
                {
                    SqlParameter sqlParameter = storedProcedureParameter[i];
                    if (i > 0)
                        parameterNames += ",";
                    parameterNames += sqlParameter.ParameterName;
                }

                var sql = string.Format("{0} {1}", storedProcedureName, string.Join(", ", storedProcedureParameter.Select(p => p.ParameterName)));

                return await _dbContextProvider.GetDbContext()
                    .Set<T>()
                    .FromSqlRaw(sql, storedProcedureParameter.ToArray()) // Pass the parameters
                    .SingleOrDefaultAsync(); // Return a single result or null
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<T> GetAsync<T>(string sql) where T : class
        {
            try
            {
                return await _dbContextProvider.GetDbContext().Set<T>().FromSqlRaw(sql).SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<string> GetNextCode(string tableName, string codeField)
        //{
        //    string addWhere = "";

        //    List<SqlParameter> sqlParameters = new List<SqlParameter>();
        //    sqlParameters.Add(new SqlParameter("@TableName", tableName));
        //    sqlParameters.Add(new SqlParameter("@CodeField", codeField));
        //    sqlParameters.Add(new SqlParameter("@AddWhere", addWhere));



        //    return await GetAsync<string>("GetNextCode", sqlParameters);
        //}
        public async Task<string> GetNextCode(string tableName, string codeField, string addWhere)
        {
            string _addWhere = addWhere ?? string.Empty;

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@TableName", tableName));
            sqlParameters.Add(new SqlParameter("@CodeField", codeField));
            sqlParameters.Add(new SqlParameter("@AddWhere", _addWhere));

            return await GetAsync<string>("GetNextCode", sqlParameters);
        }
        public async Task<bool> CheckRelatedTable(string TableName, long ID, string ExceptTableName)
        {
            string parameterNames = string.Empty;
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            SqlParameter retIDParameter = new SqlParameter("@RetID", null);
            retIDParameter.DbType = DbType.Int32;
            retIDParameter.Direction = ParameterDirection.ReturnValue;
            sqlParameters.Add(retIDParameter);

            sqlParameters.Add(new SqlParameter("@TableName", TableName));
            sqlParameters.Add(new SqlParameter("@ID", ID));
            sqlParameters.Add(new SqlParameter("@ExceptTableName", ExceptTableName));

            foreach (SqlParameter sqlParameter in sqlParameters)
            {
                if (sqlParameter.Direction == ParameterDirection.ReturnValue)
                    continue;
                if (!string.IsNullOrEmpty(parameterNames))
                    parameterNames += ",";
                parameterNames += sqlParameter.ParameterName;
            }
            try
            {

                var result = await _dbContextProvider.GetDbContext()
                                             .Set<object>() // Set the DbSet for the object (adjust the type according to your needs)
                                             .FromSqlRaw("CheckRelatedTable {0}", sqlParameters.ToArray()) // Use FromSqlRaw with parameters
                                             .SingleAsync(); // Fetch a single result


                //await _dbContextProvider.GetDbContext().Database.ExecuteSqlCommandAsync("CheckRelatedTable " + parameterNames, sqlParameters.ToArray());
            }
            catch (Exception)
            {

            }
            int Result = (int)retIDParameter.Value;
            return Result > 0;
        }
        //SitePages
        public async Task<string> GetNextKey(int pageSectionId, int pageCategoryId)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@PageSectionId", pageSectionId));
            sqlParameters.Add(new SqlParameter("@PageCategoryId", pageCategoryId));
            var result = await GetAsync<string>("GetNextKey", sqlParameters);
            return result;
        }

        //////
        public DataSet GetDataSet(string storedProcedureName, List<SqlParameter> storedProcedureParameter)
        {
            DataSet result;
            using (DbDataAdapter dbDataAdapter = new SqlDataAdapter())
            {
                string parameterNames = string.Empty;
                for (int i = 0; i < storedProcedureParameter.Count; i++)
                {
                    SqlParameter sqlParameter = storedProcedureParameter[i];
                    if (i > 0)
                        parameterNames += ",";
                    parameterNames += sqlParameter.ParameterName;
                }

                dbDataAdapter.SelectCommand = _dbContextProvider.GetDbContext().Database.GetDbConnection().CreateCommand();
                dbDataAdapter.SelectCommand.CommandType = CommandType.Text;
                dbDataAdapter.SelectCommand.CommandText = String.Format("{0} {1}", storedProcedureName, parameterNames);
                dbDataAdapter.SelectCommand.Parameters.AddRange(storedProcedureParameter.ToArray());
                DataSet dataSet = new DataSet();
                dbDataAdapter.Fill(dataSet);
                result = dataSet;
            }
            return result;
        }
        public DataSet GetDataSet(string sql)
        {
            DataSet result;
            using (DbDataAdapter dbDataAdapter = new SqlDataAdapter())
            {
                dbDataAdapter.SelectCommand = _dbContextProvider.GetDbContext().Database.GetDbConnection().CreateCommand();
                dbDataAdapter.SelectCommand.CommandType = CommandType.Text;
                dbDataAdapter.SelectCommand.CommandText = sql;
                DataSet dataSet = new DataSet();
                dbDataAdapter.Fill(dataSet);
                result = dataSet;
            }
            return result;
        }



        public async Task<List<T>> ExecuteSql2Async<T>(string sql) where T : class
        {
            try
            {
                return await _dbContextProvider.GetDbContext().Set<T>().FromSqlRaw(sql).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
