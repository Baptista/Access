using Access.Data;
using Access.DataAccess;
using Microsoft.AspNetCore.Identity;

namespace Access.Services.Authentication
{

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordHasher<ApplicationUser> passwordHasher,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }




        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return false;

            var storedTokenData = await _userRepository.GetAuthenticationTokenAsync(
                user.Id, "EmailConfirm", "Token");

            if (string.IsNullOrEmpty(storedTokenData)) return false;

            var parts = storedTokenData.Split(':');
            if (parts.Length != 2) return false;

            var storedToken = parts[0];
            var expiryTicks = long.Parse(parts[1]);
            var expiry = new DateTime(expiryTicks);

            if (DateTime.UtcNow > expiry || storedToken != token)
            {
                await _userRepository.RemoveAuthenticationTokenAsync(
                    user.Id, "EmailConfirm", "Token");
                return false;
            }

            var result = await _userRepository.ConfirmEmailAsync(user.Id);
            if (result)
            {
                await _userRepository.RemoveAuthenticationTokenAsync(
                    user.Id, "EmailConfirm", "Token");
            }
            return result;
        }

        
        public async Task<ApplicationUser> ValidateUserAsync(string userName, string password)
        {
            var user = await _userRepository.GetUserByUserNameAsync(userName);
            if (user == null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            // Generate reset token
            var tokenData = $"RESET_{user.Id}_{user.Email}_{DateTime.UtcNow.Ticks}";
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
            var token = Convert.ToBase64String(tokenBytes);

            // Store token for validation
            await _userRepository.SetAuthenticationTokenAsync(user.Id, "PasswordReset", "Token", token);

            return token;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            // Validate token
            var storedToken = await _userRepository.GetAuthenticationTokenAsync(user.Id, "PasswordReset", "Token");
            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
            {
                return false;
            }

            // Hash new password
            var newPasswordHash = _passwordHasher.HashPassword(user, newPassword);

            // Update password
            var result = await _userRepository.UpdatePasswordHashAsync(user.Id, newPasswordHash);

            if (result)
            {
                // Remove the reset token
                await _userRepository.RemoveAuthenticationTokenAsync(user.Id, "PasswordReset", "Token");
            }

            return result;
        }
    }

}
