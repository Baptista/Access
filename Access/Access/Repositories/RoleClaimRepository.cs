using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;

namespace Access.Repositories
{
    public class RoleClaimRepository
    {
        private readonly string _connectionString;
        public RoleClaimRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertRoleClaimAsync(string roleId, string claimType, string claimValue, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@ClaimType", claimType),
                new SqlParameter("@ClaimValue", claimValue)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertAspNetRoleClaim", parameters, transaction);
        }

        public async Task<DataTable> GetRoleClaimsAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetAspNetRoleClaims", null, transaction);
        }
    }
}
