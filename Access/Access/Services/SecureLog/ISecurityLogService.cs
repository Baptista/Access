namespace Access.Services.SecureLog
{
    public interface ISecurityLogService
    {
        Task LogSecurityEvent(string ipAddress, string userEmail, string action, string description);
    }
}
