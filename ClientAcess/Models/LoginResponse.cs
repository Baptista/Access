namespace ClientAcess.Models
{
    public class LoginResponse
    {
        public TokenType AccessToken { get; set; }
        public string Username { get; set; }
    }
}
