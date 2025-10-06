using Access.Constants;
using Access.Models;
//using MimeKit;
//using MailKit.Net.Smtp;
using Polly.Retry;
using Polly;
using System.Net.Mail;
using System.Net;
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
            //emailMessage.Body = GetHtmlBody(message.Content);


            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(GetHtmlBody(message.Content), null, "text/html");

            // Caminho físico da imagem no disco
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "img", "logo.png");
            var logoResource = new LinkedResource(logoPath, "image/png")
            {
                ContentId = "logoImage"
            };
            avHtml.LinkedResources.Add(logoResource);

            emailMessage.AlternateViews.Add(avHtml);


            emailMessage.IsBodyHtml = true;
            return emailMessage;
        }


        private string GetHtmlBody(string content)
        {
            // Replace URLs with anchor tags
            string pattern = @"(http[s]?://[^\s<>]+)";
            string replacedContent = System.Text.RegularExpressions.Regex.Replace(content, pattern, "<a href='$1'>$1</a>");
            //<b class='logo me-auto'><img src='cid:logoImage' width='100'></b>
            return $@"
    <html>
        <body>
            <p>{replacedContent}</p>

            <br /> 
            <br /> 
            <br /> 
            Best Regards,<br /> 
            Blaykor <br /> 
            
            
            
                           
            🌐 <a href='https://www.blaykor.com' style='color:green'>https://www.blaykor.com</a> <br />            
            ✉️ support.bk@blaykor.com<br/>           

        </body>
    </html>";
        }


        private async Task SendAsync(MailMessage mailMessage)
        {
#if DEBUG

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_emailConfig.From, _emailConfig.Password);
                    client.Send(mailMessage);
                    
                }               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error send email: " + ex.Message);
            }
#else
            using var client = new SmtpClient(_emailConfig.SmtpServer);
            client.Credentials = new NetworkCredential("noreply@blaykor.com", "wrtcqx2009@Q");
            try
            {                
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

#endif
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
