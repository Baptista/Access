using ClientAcess.Models.Enums;

namespace ClientAcess.Models
{
    public class Response
    {
        public ApiCode Status { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public LoginResponse Result { get; set; }
    }
}
