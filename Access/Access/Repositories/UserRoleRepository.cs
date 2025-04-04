using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;

namespace Access.Repositories
{
    public class UserRoleRepository
    {

        private readonly string _connectionString;
        public UserRoleRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertUserRoleAsync(string userId, string roleId, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RoleId", roleId)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertAspNetUserRole", parameters, transaction);
        }

        public async Task<DataTable> GetUserRolesAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetAspNetUserRoles", null, transaction);
        }
    }
}
