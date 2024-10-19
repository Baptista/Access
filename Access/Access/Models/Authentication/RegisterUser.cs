using Access.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Access.Models.Authentication
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [PasswordComplexity]
        public string? Password { get; set; }

        public List<string>? Roles { get; set; }

    }
}
