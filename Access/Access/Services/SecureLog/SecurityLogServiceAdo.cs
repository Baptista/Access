using System.Threading.Tasks;
using Access.DataAccess;
using Access.Models;
using Microsoft.Data.SqlClient;

namespace Access.Services.SecureLog
{
    public class SecurityLogServiceAdo : ISecurityLogService
    {
        private readonly ISecurityLogRepository _securityLogRepository;
        private readonly string _connectionString;

        public SecurityLogServiceAdo(ISecurityLogRepository securityLogRepository, IConfiguration configuration)
        {
            _securityLogRepository = securityLogRepository;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SecurityLog>> GetLogsBetweenDates(DateTime start, DateTime end)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            return await _securityLogRepository.GetSecurityLogsAsync(start, end, null, connection);
        }

        public async Task LogSecurityEvent(string ipAddress, string userEmail, string action, string description)
        {
            // Security logs don't need transactions typically, but you can pass connection if needed
            await _securityLogRepository.LogSecurityEventAsync(ipAddress, userEmail, action, description);
        }
    }
}