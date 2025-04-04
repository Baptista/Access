using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;


namespace Access.Repositories
{
    public class UserLoginRepository
    {
        private readonly string _connectionString;
        public UserLoginRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertUserLoginAsync(string loginProvider, string providerKey, string providerDisplayName, string userId, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@LoginProvider", loginProvider),
                new SqlParameter("@ProviderKey", providerKey),
                new SqlParameter("@ProviderDisplayName", providerDisplayName),
                new SqlParameter("@UserId", userId)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertAspNetUserLogin", parameters, transaction);
        }

        public async Task<DataTable> GetUserLoginsAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetAspNetUserLogins", null, transaction);
        }
    }
}
