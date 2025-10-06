using System;
using System.Threading.Tasks;
using Access.Data;
using Access.DataAccess;
using Access.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Access.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<ApplicationUser> ValidateUserAsync(string userName, string password);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
