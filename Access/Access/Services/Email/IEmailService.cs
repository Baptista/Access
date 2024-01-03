using Access.Models;

namespace Access.Services.Email
{
    public interface IEmailService
    {
        string SendEmail(Message message);
    }
}
