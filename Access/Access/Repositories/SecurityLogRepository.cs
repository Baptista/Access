using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;

namespace Access.Repositories
{
    public class SecurityLogRepository
    {
        private readonly string _connectionString;
        public SecurityLogRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertSecurityLogAsync(string ipAddress, string email, string description, DateTime createdOn, string action, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@IpAddress", ipAddress),
                new SqlParameter("@Email", email),
                new SqlParameter("@Description", description),
                new SqlParameter("@CreatedOn", createdOn),
                new SqlParameter("@Action", action)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertSecurityLog", parameters, transaction);
        }

        public async Task<DataTable> GetSecurityLogsAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetSecurityLogs", null, transaction);
        }
    }
}
