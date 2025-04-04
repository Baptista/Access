using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;


namespace Access.Repositories
{
    public class UserClaimRepository
    {
        private readonly string _connectionString;
        public UserClaimRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertUserClaimAsync(string userId, string claimType, string claimValue, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ClaimType", claimType),
                new SqlParameter("@ClaimValue", claimValue)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertAspNetUserClaim", parameters, transaction);
        }

        public async Task<DataTable> GetUserClaimsAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetAspNetUserClaims", null, transaction);
        }
    }
}
