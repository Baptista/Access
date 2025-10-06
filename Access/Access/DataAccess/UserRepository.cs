// File: Access/DataAccess/UserRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.Models;
using Access.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Linq;
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

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserByEmail", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Email", email);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }
            return null;
        }

        public async Task<ApplicationUser> GetUserByUserNameAsync(string userName)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserByUserName", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserName", userName);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }
            return null;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }
            return null;
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CreateUser", connection);
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

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_UpdateUser", connection);
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

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> ConfirmEmailAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ConfirmUserEmail", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> UpdatePasswordHashAsync(string userId, string passwordHash)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_UpdatePasswordHash", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<int> IncrementAccessFailedCountAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_IncrementAccessFailedCount", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> ResetAccessFailedCountAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ResetAccessFailedCount", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> SetLockoutEndDateAsync(string userId, DateTimeOffset? lockoutEnd)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_SetLockoutEndDate", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);
            command.Parameters.AddWithValue("@LockoutEnd", (object)lockoutEnd ?? DBNull.Value);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> CheckPasswordAsync(string userId, string passwordHash)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            // In a real implementation, you would use the password hasher to verify
            return user.PasswordHash == passwordHash;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var roles = new List<string>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserRoles", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                roles.Add(reader["Name"].ToString());
            }
            return roles;
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_IsUserInRole", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleName", roleName);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            using var connection = new SqlConnection(_connectionString);

            // First get the role ID
            var roleId = await GetRoleIdByNameAsync(connection, roleName);
            if (string.IsNullOrEmpty(roleId)) return false;

            using var command = new SqlCommand("sp_AddUserToRole", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            using var connection = new SqlConnection(_connectionString);

            // First get the role ID
            var roleId = await GetRoleIdByNameAsync(connection, roleName);
            if (string.IsNullOrEmpty(roleId)) return false;

            using var command = new SqlCommand("sp_RemoveUserFromRole", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<string> GetAuthenticationTokenAsync(string userId, string loginProvider, string name)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetAuthenticationToken", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@LoginProvider", loginProvider);
            command.Parameters.AddWithValue("@Name", name);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<bool> SetAuthenticationTokenAsync(string userId, string loginProvider, string name, string value)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_SetAuthenticationToken", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@LoginProvider", loginProvider);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Value", value);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> RemoveAuthenticationTokenAsync(string userId, string loginProvider, string name)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_RemoveAuthenticationToken", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@LoginProvider", loginProvider);
            command.Parameters.AddWithValue("@Name", name);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
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

        private async Task<string> GetRoleIdByNameAsync(SqlConnection connection, string roleName)
        {
            using var command = new SqlCommand("sp_GetRoleByName", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Name", roleName);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader["Id"].ToString();
            }
            return null;
        }

        public async Task<LockoutResult> HandleFailedLoginAsync(
     string userId,
     int maxFailedAttempts = 3,
     int lockoutMinutes = 5)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_HandleFailedLogin", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id", userId);
            command.Parameters.AddWithValue("@MaxFailedAttempts", maxFailedAttempts);
            command.Parameters.AddWithValue("@LockoutMinutes", lockoutMinutes);

            await connection.OpenAsync();

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

            // Fallback (shouldn't happen if user exists)
            return new LockoutResult { FailedCount = 0, IsLockedOut = false };
        }


        public async Task<bool> HandleSuccessfulLoginAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_HandleSuccessfulLogin", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<LockoutStatus> CheckUserLockoutAsync(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CheckUserLockout", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);

            await connection.OpenAsync();
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

        public async Task<UserWithLockoutInfo> GetUserWithLockoutInfoAsync(string userName)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserWithLockoutInfo", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserName", userName);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var user = new UserWithLockoutInfo
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

                return user;
            }

            return null;
        }

        public async Task<int> ClearExpiredLockoutsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClearExpiredLockouts", connection);
            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}



