using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.DataAccess;


namespace Access.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;
        public UserRepository(string connectionString) => _connectionString = connectionString;

        public async Task InsertUserAsync(
            string id, string userName, string normalizedUserName,
            string email, string normalizedEmail, bool emailConfirmed,
            string passwordHash, string securityStamp, string concurrencyStamp,
            string phoneNumber, bool phoneNumberConfirmed, bool twoFactorEnabled,
            DateTime? lockoutEnd, bool lockoutEnabled, int accessFailedCount,
            SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@UserName", userName),
                new SqlParameter("@NormalizedUserName", normalizedUserName),
                new SqlParameter("@Email", email),
                new SqlParameter("@NormalizedEmail", normalizedEmail),
                new SqlParameter("@EmailConfirmed", emailConfirmed),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@SecurityStamp", securityStamp),
                new SqlParameter("@ConcurrencyStamp", concurrencyStamp),
                new SqlParameter("@PhoneNumber", phoneNumber),
                new SqlParameter("@PhoneNumberConfirmed", phoneNumberConfirmed),
                new SqlParameter("@TwoFactorEnabled", twoFactorEnabled),
                new SqlParameter("@LockoutEnd", lockoutEnd.HasValue ? (object)lockoutEnd.Value : DBNull.Value),
                new SqlParameter("@LockoutEnabled", lockoutEnabled),
                new SqlParameter("@AccessFailedCount", accessFailedCount)
            };

            await DatabaseHelper.ExecuteNonQueryAsync(_connectionString, "sp_InsertAspNetUser", parameters, transaction);
        }

        public async Task<DataTable> GetUsersAsync(SqlTransaction transaction = null)
        {
            return await DatabaseHelper.ExecuteQueryAsync(_connectionString, "sp_GetAspNetUsers", null, transaction);
        }
    }
}
