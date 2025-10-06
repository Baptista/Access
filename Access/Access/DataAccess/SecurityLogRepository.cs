using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Access.Models;

namespace Access.DataAccess
{
    public class SecurityLogRepository : ISecurityLogRepository
    {
        private readonly string _connectionString;

        public SecurityLogRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> LogSecurityEventAsync(string ipAddress, string userEmail, string action, string description)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_InsertSecurityLog", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IpAddress", ipAddress ?? "Unknown");
            command.Parameters.AddWithValue("@Email", userEmail ?? "Unknown");
            command.Parameters.AddWithValue("@Description", description ?? string.Empty);
            command.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
            command.Parameters.AddWithValue("@Action", action ?? string.Empty);

            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<List<SecurityLog>> GetSecurityLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string email = null)
        {
            var logs = new List<SecurityLog>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetSecurityLogs", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@StartDate", (object)startDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(new SecurityLog
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    IpAddress = reader["IpAddress"].ToString(),
                    UserEmail = reader["Email"].ToString(),
                    Action = reader["Action"].ToString(),
                    Description = reader["Description"].ToString(),
                    CreatedOn = Convert.ToDateTime(reader["CreatedOn"])
                });
            }
            return logs;
        }
    }
}
