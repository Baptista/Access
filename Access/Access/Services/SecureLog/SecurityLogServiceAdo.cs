using System.Threading.Tasks;
using Access.DataAccess;
using Access.Models;

namespace Access.Services.SecureLog
{
    public class SecurityLogServiceAdo : ISecurityLogService
    {
        private readonly ISecurityLogRepository _securityLogRepository;

        public SecurityLogServiceAdo(ISecurityLogRepository securityLogRepository)
        {
            _securityLogRepository = securityLogRepository;
        }

        public Task<List<SecurityLog>> GetLogsBetweenDates(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public async Task LogSecurityEvent(string ipAddress, string userEmail, string action, string description)
        {
            await _securityLogRepository.LogSecurityEventAsync(ipAddress, userEmail, action, description);
        }
    }
}