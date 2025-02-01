using Access.Data;
using Access.Models;

namespace Access.Services.SecureLog
{
    public class SecurityLogService : ISecurityLogService
    {
        private readonly DataContext _context;

        public SecurityLogService(DataContext context)
        {
            _context = context;
        }

        public async Task LogSecurityEvent(string ipAddress, string userEmail, string action, string description)
        {
            var logEntry = new SecurityLog
            {
                IpAddress = ipAddress,
                UserEmail = userEmail,
                Action = action,
                Description = description
            };

            _context.SecurityLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}
