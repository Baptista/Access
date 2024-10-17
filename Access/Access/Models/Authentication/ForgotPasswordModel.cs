using System.ComponentModel.DataAnnotations;

namespace Access.Models.Authentication
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
