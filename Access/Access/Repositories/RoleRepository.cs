using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;

namespace Access.Repositories
{
    public class RoleRepository
    {
        private readonly string _connectionString;
        public RoleRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertRoleAsync(string id, string name, string normalizedName, string concurrencyStamp, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Name", name),
                new SqlParameter("@NormalizedName", normalizedName),
                new SqlParameter("@ConcurrencyStamp", concurrencyStamp)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertAspNetRole", parameters, transaction);
        }

        public async Task<DataTable> GetRolesAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetAspNetRoles", null, transaction);
        }
    }
}
