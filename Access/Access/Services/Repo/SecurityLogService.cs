using Access.Repositories;
using System.Data;

namespace Access.Services.Repo
{
    public class SecurityLogService
    {
        private readonly SecurityLogRepository _securityLogRepository;

        public SecurityLogService(string connectionString)
        {
            _securityLogRepository = new SecurityLogRepository(connectionString);
        }

        public async Task InsertSecurityLogAsync(string ipAddress, string email, string description, DateTime createdOn, string action)
        {
            await _securityLogRepository.InsertSecurityLogAsync(ipAddress, email, description, createdOn, action);
        }

        public async Task<DataTable> GetSecurityLogsAsync()
        {
            return await _securityLogRepository.GetSecurityLogsAsync();
        }
    }
}
