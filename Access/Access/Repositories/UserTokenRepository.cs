using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;

namespace Access.Repositories
{
    public class UserTokenRepository
    {
        private readonly string _connectionString;
        public UserTokenRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertUserTokenAsync(string userId, string loginProvider, string name, string value, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LoginProvider", loginProvider),
                new SqlParameter("@Name", name),
                new SqlParameter("@Value", value)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertAspNetUserToken", parameters, transaction);
        }

        public async Task<DataTable> GetUserTokensAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetAspNetUserTokens", null, transaction);
        }
    }
}
