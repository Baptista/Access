using System.ComponentModel.DataAnnotations;

namespace Access.Models.Authentication
{
    public class LoginWithOtpModel
    {
        public string UserName { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
