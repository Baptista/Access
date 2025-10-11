using Access.Data;
using Access.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace Access.Services.Authentication
{

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly string _connectionString;

        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordHasher<ApplicationUser> passwordHasher,
            ILogger<AuthenticationService> logger,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email, connection, transaction);
                if (user == null)
                {
                    transaction.Rollback();
                    return false;
                }

                // Validate token
                var expectedTokenData = $"{user.Id}_{user.Email}_";
                var tokenBytes = Convert.FromBase64String(token);
                var tokenData = System.Text.Encoding.UTF8.GetString(tokenBytes);

                if (!tokenData.StartsWith(expectedTokenData))
                {
                    transaction.Rollback();
                    return false;
                }

                var result = await _userRepository.ConfirmEmailAsync(user.Id, connection, transaction);

                if (result)
                {
                    // Remove confirmation token after successful confirmation
                    await _userRepository.RemoveAuthenticationTokenAsync(user.Id, "EmailConfirm", "Token", connection, transaction);
                    transaction.Commit();
                    return true;
                }

                transaction.Rollback();
                return false;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error confirming email");
                throw;
            }
        }

        public async Task<ApplicationUser> ValidateUserAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                var user = await _userRepository.GetUserByUserNameAsync(userName, connection);
                if (user == null)
                {
                    return null;
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                return result == PasswordVerificationResult.Success ? user : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user");
                throw;
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Generate reset token
                var tokenData = $"RESET_{user.Id}_{user.Email}_{DateTime.UtcNow.Ticks}";
                var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
                var token = Convert.ToBase64String(tokenBytes);

                // Store token for validation
                await _userRepository.SetAuthenticationTokenAsync(user.Id, "PasswordReset", "Token", token, connection, transaction);

                transaction.Commit();
                return token;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error generating password reset token");
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email, connection, transaction);
                if (user == null)
                {
                    transaction.Rollback();
                    return false;
                }

                // Validate token
                var storedToken = await _userRepository.GetAuthenticationTokenAsync(user.Id, "PasswordReset", "Token", connection, transaction);
                if (string.IsNullOrEmpty(storedToken) || storedToken != token)
                {
                    transaction.Rollback();
                    return false;
                }

                // Hash new password
                var newPasswordHash = _passwordHasher.HashPassword(user, newPassword);

                // Update password
                var result = await _userRepository.UpdatePasswordHashAsync(user.Id, newPasswordHash, connection, transaction);

                if (result)
                {
                    // Remove the reset token
                    await _userRepository.RemoveAuthenticationTokenAsync(user.Id, "PasswordReset", "Token", connection, transaction);

                    // Reset failed login attempts
                    await _userRepository.ResetAccessFailedCountAsync(user.Id, connection, transaction);

                    transaction.Commit();
                    return true;
                }

                transaction.Rollback();
                return false;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error resetting password");
                throw;
            }
        }
    }

}
