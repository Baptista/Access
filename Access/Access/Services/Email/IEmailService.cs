using Access.Models;

namespace Access.Services.Email
{
    public interface IEmailService
    {
        Task<EmailResponse> SendEmailAsync(Message message);
    }
}
