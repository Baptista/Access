namespace Access.Constants
{
    public static class ResponseMessages
    {
        public static string GetEmailSuccessMessage(string emailAddress) => $"Email sent successfully to {emailAddress}";
        public static string GetEmailFailureMessage(string emailAddress) => $"Error sending email to {emailAddress}";
    }
}
