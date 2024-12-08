using Access.Constants;
using Access.Models;
//using MimeKit;
//using MailKit.Net.Smtp;
using Polly.Retry;
using Polly;
using System.Net.Mail;
//using System.Net.Mail;

namespace Access.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public EmailService(EmailConfiguration emailConfig, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig;
            _logger = logger;

            // Define a retry policy with Polly: retry 3 times with an exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for email sending. Exception: {exception.Message}");
                    });
        }

        public async Task<EmailResponse> SendEmailAsync(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            try
            {
                await _retryPolicy.ExecuteAsync(() => SendAsync(emailMessage));
                var recipients = string.Join(", ", message.To);
                return new EmailResponse(true, $"Email sent successfully to {recipients}");
            }
            catch (Exception ex)
            {
                // Log the error with safe information, avoid exposing sensitive data
                _logger.LogError($"Error sending email: {ex.Message}. StackTrace: {ex.StackTrace}");

                var recipients = string.Join(", ", message.To);
                return new EmailResponse(false, $"Error sending email to {recipients}");
            }
        }

        private MailMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MailMessage();
            emailMessage.From = new MailAddress(_emailConfig.From);            
            emailMessage.To.Add(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = message.Content;

            return emailMessage;
        }

        private async Task SendAsync(MailMessage mailMessage)
        {
            using var client = new SmtpClient(_emailConfig.SmtpServer);
            try
            {
                //await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                //client.AuthenticationMechanisms.Remove("XOAUTH2");
                //await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                client.Send(mailMessage);
                _logger.LogInformation("Email sent successfully.");
            }            
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                throw; // Rethrow to ensure Polly handles the retries
            }
            finally
            {                
                client.Dispose();
            }
        }
    }

    public class EmailResponse
    {
        private bool isSuccess;
        private string message;

        public EmailResponse(bool isSuccess, string message)
        {
            this.isSuccess = isSuccess;
            this.message = message;
        }
        public bool Success => isSuccess;
        public string Message => message;
    }
}
