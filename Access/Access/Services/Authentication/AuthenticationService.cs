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
            if (user == null)
            {
                return false;
            }

            // Validate token (simplified - in production use proper token validation)
            var expectedTokenData = $"{user.Id}_{user.Email}_";
            var tokenBytes = Convert.FromBase64String(token);
            var tokenData = System.Text.Encoding.UTF8.GetString(tokenBytes);

            if (!tokenData.StartsWith(expectedTokenData))
            {
                return false;
            }

            return await _userRepository.ConfirmEmailAsync(user.Id);
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
