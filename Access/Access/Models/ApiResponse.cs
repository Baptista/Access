using Access.Constants;

namespace Access.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public T? Response { get; set; }
        public ApiCode InternalCode { get; set; }
    }
}
