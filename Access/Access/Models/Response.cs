using Access.Constants;

namespace Access.Models
{
    public class Response
    {
        public ApiCode Status { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public object? Result { get; set; }
    }
}
