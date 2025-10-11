// File: Access/DataAccess/UserRepository.cs - COMPLETE IMPLEMENTATION
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Access.Models.Lockout;

namespace Access.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public UserRepository(IConfiguration configuration, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _passwordHasher = passwordHasher;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_GetUserByEmail", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Email", email);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUserFromReader(reader);
                }
                return null;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<ApplicationUser> GetUserByUserNameAsync(string userName, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_GetUserByUserName", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserName", userName);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUserFromReader(reader);
                }
                return null;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_GetUserById", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUserFromReader(reader);
                }
                return null;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_CreateUser", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Id", user.Id ?? Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("@UserName", user.UserName ?? string.Empty);
                command.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName ?? user.UserName?.ToUpper() ?? string.Empty);
                command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
                command.Parameters.AddWithValue("@NormalizedEmail", user.NormalizedEmail ?? user.Email?.ToUpper() ?? string.Empty);
                command.Parameters.AddWithValue("@EmailConfirmed", user.EmailConfirmed);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? string.Empty);
                command.Parameters.AddWithValue("@SecurityStamp", user.SecurityStamp ?? Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("@ConcurrencyStamp", user.ConcurrencyStamp ?? Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@PhoneNumberConfirmed", user.PhoneNumberConfirmed);
                command.Parameters.AddWithValue("@TwoFactorEnabled", user.TwoFactorEnabled);
                command.Parameters.AddWithValue("@LockoutEnd", (object)user.LockoutEnd ?? DBNull.Value);
                command.Parameters.AddWithValue("@LockoutEnabled", user.LockoutEnabled);
                command.Parameters.AddWithValue("@AccessFailedCount", user.AccessFailedCount);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_UpdateUser", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@UserName", user.UserName ?? string.Empty);
                command.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName ?? user.UserName?.ToUpper() ?? string.Empty);
                command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
                command.Parameters.AddWithValue("@NormalizedEmail", user.NormalizedEmail ?? user.Email?.ToUpper() ?? string.Empty);
                command.Parameters.AddWithValue("@EmailConfirmed", user.EmailConfirmed);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? string.Empty);
                command.Parameters.AddWithValue("@SecurityStamp", user.SecurityStamp ?? string.Empty);
                command.Parameters.AddWithValue("@ConcurrencyStamp", user.ConcurrencyStamp ?? string.Empty);
                command.Parameters.AddWithValue("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@PhoneNumberConfirmed", user.PhoneNumberConfirmed);
                command.Parameters.AddWithValue("@TwoFactorEnabled", user.TwoFactorEnabled);
                command.Parameters.AddWithValue("@LockoutEnd", (object)user.LockoutEnd ?? DBNull.Value);
                command.Parameters.AddWithValue("@LockoutEnabled", user.LockoutEnabled);
                command.Parameters.AddWithValue("@AccessFailedCount", user.AccessFailedCount);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_ConfirmUserEmail", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> UpdatePasswordHashAsync(string userId, string passwordHash, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_UpdatePasswordHash", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<int> IncrementAccessFailedCountAsync(string userId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_IncrementAccessFailedCount", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> ResetAccessFailedCountAsync(string userId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_ResetAccessFailedCount", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> SetLockoutEndDateAsync(string userId, DateTimeOffset? lockoutEnd, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_SetLockoutEndDate", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);
                command.Parameters.AddWithValue("@LockoutEnd", (object)lockoutEnd ?? DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> CheckPasswordAsync(string userId, string passwordHash, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            var user = await GetUserByIdAsync(userId, connection, transaction);
            if (user == null) return false;

            return user.PasswordHash == passwordHash;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;
            var roles = new List<string>();

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_GetUserRoles", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    roles.Add(reader["Name"].ToString());
                }
                return roles;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_IsUserInRole", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@RoleName", roleName);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                // First get the role ID
                var roleId = await GetRoleIdByNameAsync(roleName, connection, transaction);
                if (string.IsNullOrEmpty(roleId)) return false;

                using var command = new SqlCommand("sp_AddUserToRole", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@RoleId", roleId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                // First get the role ID
                var roleId = await GetRoleIdByNameAsync(roleName, connection, transaction);
                if (string.IsNullOrEmpty(roleId)) return false;

                using var command = new SqlCommand("sp_RemoveUserFromRole", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@RoleId", roleId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<string> GetAuthenticationTokenAsync(string userId, string loginProvider, string name, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_GetAuthenticationToken", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@LoginProvider", loginProvider);
                command.Parameters.AddWithValue("@Name", name);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> SetAuthenticationTokenAsync(string userId, string loginProvider, string name, string value, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_SetAuthenticationToken", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@LoginProvider", loginProvider);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Value", value);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> RemoveAuthenticationTokenAsync(string userId, string loginProvider, string name, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_RemoveAuthenticationToken", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@LoginProvider", loginProvider);
                command.Parameters.AddWithValue("@Name", name);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<LockoutResult> HandleFailedLoginAsync(string userId, int maxFailedAttempts = 3, int lockoutMinutes = 5, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_HandleFailedLogin", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);
                command.Parameters.AddWithValue("@MaxFailedAttempts", maxFailedAttempts);
                command.Parameters.AddWithValue("@LockoutMinutes", lockoutMinutes);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new LockoutResult
                    {
                        FailedCount = reader.GetInt32(reader.GetOrdinal("FailedCount")),
                        IsLockedOut = reader.GetBoolean(reader.GetOrdinal("IsLockedOut")),
                        LockoutEnd = reader.IsDBNull(reader.GetOrdinal("LockoutEnd"))
                            ? null
                            : reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("LockoutEnd"))
                    };
                }

                return new LockoutResult { FailedCount = 0, IsLockedOut = false };
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<bool> HandleSuccessfulLoginAsync(string userId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_HandleSuccessfulLogin", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<LockoutStatus> CheckUserLockoutAsync(string userId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_CheckUserLockout", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", userId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new LockoutStatus
                    {
                        IsLockedOut = Convert.ToBoolean(reader["IsLockedOut"]),
                        LockoutEnd = reader["LockoutEnd"] != DBNull.Value ? (DateTimeOffset?)reader["LockoutEnd"] : null,
                        RemainingMinutes = Convert.ToInt32(reader["RemainingMinutes"])
                    };
                }

                return new LockoutStatus { IsLockedOut = false };
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<UserWithLockoutInfo> GetUserWithLockoutInfoAsync(string userName, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_GetUserWithLockoutInfo", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserName", userName);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new UserWithLockoutInfo
                    {
                        Id = reader["Id"].ToString(),
                        UserName = reader["UserName"].ToString(),
                        NormalizedUserName = reader["NormalizedUserName"].ToString(),
                        Email = reader["Email"].ToString(),
                        NormalizedEmail = reader["NormalizedEmail"].ToString(),
                        EmailConfirmed = Convert.ToBoolean(reader["EmailConfirmed"]),
                        PasswordHash = reader["PasswordHash"].ToString(),
                        SecurityStamp = reader["SecurityStamp"].ToString(),
                        ConcurrencyStamp = reader["ConcurrencyStamp"].ToString(),
                        PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : null,
                        PhoneNumberConfirmed = Convert.ToBoolean(reader["PhoneNumberConfirmed"]),
                        TwoFactorEnabled = Convert.ToBoolean(reader["TwoFactorEnabled"]),
                        LockoutEnd = reader["LockoutEnd"] != DBNull.Value ? (DateTimeOffset?)reader["LockoutEnd"] : null,
                        LockoutEnabled = Convert.ToBoolean(reader["LockoutEnabled"]),
                        AccessFailedCount = Convert.ToInt32(reader["AccessFailedCount"]),
                        IsCurrentlyLockedOut = Convert.ToBoolean(reader["IsCurrentlyLockedOut"]),
                        LockoutRemainingMinutes = Convert.ToInt32(reader["LockoutRemainingMinutes"])
                    };
                }

                return null;
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<int> ClearExpiredLockoutsAsync(SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool shouldCloseConnection = false;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    shouldCloseConnection = true;
                }

                using var command = new SqlCommand("sp_ClearExpiredLockouts", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        private async Task<string> GetRoleIdByNameAsync(string roleName, SqlConnection connection, SqlTransaction transaction = null)
        {
            using var command = new SqlCommand("sp_GetRoleByName", connection, transaction);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Name", roleName);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader["Id"].ToString();
            }
            return null;
        }

        private ApplicationUser MapUserFromReader(SqlDataReader reader)
        {
            return new ApplicationUser
            {
                Id = reader["Id"].ToString(),
                UserName = reader["UserName"].ToString(),
                NormalizedUserName = reader["NormalizedUserName"].ToString(),
                Email = reader["Email"].ToString(),
                NormalizedEmail = reader["NormalizedEmail"].ToString(),
                EmailConfirmed = Convert.ToBoolean(reader["EmailConfirmed"]),
                PasswordHash = reader["PasswordHash"].ToString(),
                SecurityStamp = reader["SecurityStamp"].ToString(),
                ConcurrencyStamp = reader["ConcurrencyStamp"].ToString(),
                PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : null,
                PhoneNumberConfirmed = Convert.ToBoolean(reader["PhoneNumberConfirmed"]),
                TwoFactorEnabled = Convert.ToBoolean(reader["TwoFactorEnabled"]),
                LockoutEnd = reader["LockoutEnd"] != DBNull.Value ? (DateTimeOffset?)reader["LockoutEnd"] : null,
                LockoutEnabled = Convert.ToBoolean(reader["LockoutEnabled"]),
                AccessFailedCount = Convert.ToInt32(reader["AccessFailedCount"])
            };
        }
    }
}