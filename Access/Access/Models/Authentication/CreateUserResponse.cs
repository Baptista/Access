using Access.Data;

namespace Access.Models.Authentication
{
    public class CreateUserResponse
    {
        public string Token { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
