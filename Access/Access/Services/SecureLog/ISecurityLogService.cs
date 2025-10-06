using Access.Models;

namespace Access.Services.SecureLog
{
    public interface ISecurityLogService
    {
        Task LogSecurityEvent(string ipAddress, string userEmail, string action, string description);
        Task<List<SecurityLog>> GetLogsBetweenDates(DateTime start, DateTime end);
    }
}
