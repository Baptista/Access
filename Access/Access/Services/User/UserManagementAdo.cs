// File: Access/Services/User/UserManagementAdo.cs
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Access.Constants;
using Access.Data;
using Access.DataAccess;
using Access.Models;
using Access.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Access.Services.User
{
    public class UserManagementAdo : IUserManagement
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserManagementAdo> _logger;
        private readonly HttpClient _httpClient;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly string _connectionString;

        public UserManagementAdo(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<UserManagementAdo> logger,
            HttpClient httpClient,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _passwordHasher = passwordHasher;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<UserResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Check if the user or username already exists
                var userExist = await _userRepository.GetUserByEmailAsync(registerUser.Email, connection, transaction);
                if (userExist != null)
                {
                    transaction.Rollback();
                    return new UserResponse<CreateUserResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Email already exists!",
                        InternalCode = ApiCode.EmailAlreadyExists
                    };
                }

                var usernameExist = await _userRepository.GetUserByUserNameAsync(registerUser.Username, connection, transaction);
                if (usernameExist != null)
                {
                    transaction.Rollback();
                    return new UserResponse<CreateUserResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Username already exists!",
                        InternalCode = ApiCode.UserAlreadyExists
                    };
                }

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = registerUser.Email,
                    NormalizedEmail = registerUser.Email.ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = registerUser.Username,
                    NormalizedUserName = registerUser.Username.ToUpper(),
                    TwoFactorEnabled = true,
                    EmailConfirmed = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                // Hash the password
                user.PasswordHash = _passwordHasher.HashPassword(user, registerUser.Password);

                var result = await _userRepository.CreateUserAsync(user, connection, transaction);
                if (!result)
                {
                    transaction.Rollback();
                    _logger.LogError("Failed to create user in database");
                    return new UserResponse<CreateUserResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "User creation failed",
                        InternalCode = ApiCode.UserCreationFailed
                    };
                }

                // Assign roles if provided, otherwise assign default "User" role
                if (registerUser.Roles != null && registerUser.Roles.Any())
                {
                    foreach (var role in registerUser.Roles)
                    {
                        await _userRepository.AddUserToRoleAsync(user.Id, role, connection, transaction);
                    }
                }
                else
                {
                    await _userRepository.AddUserToRoleAsync(user.Id, "User", connection, transaction);
                }

                // Commit transaction
                transaction.Commit();

                // Generate email confirmation token (outside transaction)
                var token = GenerateEmailConfirmationToken(user);

                return new UserResponse<CreateUserResponse>
                {
                    Response = new CreateUserResponse { User = user, Token = token },
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "User created and confirmation token generated",
                    InternalCode = ApiCode.Success
                };
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error creating user with token");
                throw;
            }
        }

        public async Task<UserResponse<LoginResponse>> GetJwtTokenAsync(ApplicationUser user)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var userRoles = await _userRepository.GetUserRolesAsync(user.Id, connection);
                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var jwtToken = GetToken(authClaims);

                return new UserResponse<LoginResponse>
                {
                    Response = new LoginResponse
                    {
                        AccessToken = new TokenType
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                            ExpiryTokenDate = jwtToken.ValidTo
                        }
                    },
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Token created",
                    InternalCode = ApiCode.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token");
                throw;
            }
        }

        public async Task<UserResponse<LoginResponse>> LoginUserWithJWTokenAsync(string otp, string userName)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var user = await _userRepository.GetUserByUserNameAsync(userName, connection, transaction);
                if (user == null)
                {
                    transaction.Rollback();
                    return new UserResponse<LoginResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "Invalid userName",
                        InternalCode = ApiCode.UserNotFound
                    };
                }

                var storedToken = await _userRepository.GetAuthenticationTokenAsync(user.Id, "Email", "OTP", connection, transaction);
                if (string.IsNullOrEmpty(storedToken) || storedToken != otp)
                {
                    transaction.Rollback();
                    return new UserResponse<LoginResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "Invalid OTP",
                        InternalCode = ApiCode.InvalidOTP
                    };
                }

                // Remove the OTP after successful validation
                await _userRepository.RemoveAuthenticationTokenAsync(user.Id, "Email", "OTP", connection, transaction);

                // Reset failed login attempts
                await _userRepository.HandleSuccessfulLoginAsync(user.Id, connection, transaction);

                transaction.Commit();

                return await GetJwtTokenAsync(user);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error during JWT login");
                throw;
            }
        }

        public async Task<UserResponse<bool>> CheckSubscription(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                var user = await _userRepository.GetUserByUserNameAsync(username, connection);
                if (user == null)
                {
                    return new UserResponse<bool>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User does not exist.",
                        InternalCode = ApiCode.UserNotFound,
                        Response = false
                    };
                }

                var emailDomain = user.Email.Split('@');
                if (emailDomain.Length != 2)
                {
                    return new UserResponse<bool>
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "Unexpected error",
                        InternalCode = ApiCode.Error
                    };
                }

                var response = await _httpClient.GetAsync($"https://subscription.keydevteam.com/api/subscription/check?domain={emailDomain[1]}");
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Subscription check response: {errorContent}");

                if (response.IsSuccessStatusCode)
                {
                    return new UserResponse<bool>
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        StatusCode = 200,
                        Message = "",
                        InternalCode = ApiCode.Success,
                        Response = response.IsSuccessStatusCode
                    };
                }
                else
                {
                    return new UserResponse<bool>
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        StatusCode = 200,
                        Message = "Subscription expired",
                        InternalCode = ApiCode.SubscriptionExpired,
                        Response = response.IsSuccessStatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subscription");
                throw;
            }
        }

        public async Task<UserResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var user = await _userRepository.GetUserByUserNameAsync(loginModel.Username, connection, transaction);
                if (user == null)
                {
                    transaction.Rollback();
                    return new UserResponse<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "User does not exist.",
                        InternalCode = ApiCode.UserNotFound
                    };
                }

                // Check if the account is currently locked out
                var lockoutStatus = await _userRepository.CheckUserLockoutAsync(user.Id, connection, transaction);
                if (lockoutStatus.IsLockedOut)
                {
                    transaction.Rollback();
                    _logger.LogWarning($"Login attempt for locked account {loginModel.Username}. Locked for {lockoutStatus.RemainingMinutes} more minutes.");
                    return new UserResponse<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 423,
                        Message = $"Account is locked due to multiple failed login attempts. Please try again in {lockoutStatus.RemainingMinutes} minute(s).",
                        InternalCode = ApiCode.UserAccountLocked
                    };
                }

                if (!user.EmailConfirmed)
                {
                    transaction.Rollback();
                    _logger.LogWarning($"Login failed for {loginModel.Username}. Email not confirmed.");
                    return new UserResponse<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "Email not confirmed. Please check your email and confirm your account.",
                        InternalCode = ApiCode.EmailNotConfirmed
                    };
                }

                // Verify password
                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginModel.Password);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    // Handle failed login attempt
                    var lockoutResult = await _userRepository.HandleFailedLoginAsync(user.Id, maxFailedAttempts: 3, lockoutMinutes: 5, connection, transaction);

                    transaction.Commit();

                    if (lockoutResult.IsLockedOut)
                    {
                        _logger.LogWarning($"Account {loginModel.Username} has been locked after {lockoutResult.FailedCount} failed attempts.");
                        return new UserResponse<LoginOtpResponse>
                        {
                            IsSuccess = false,
                            StatusCode = 423,
                            Message = $"Account has been locked due to {lockoutResult.FailedCount} failed login attempts. Please try again in 5 minutes.",
                            InternalCode = ApiCode.UserAccountLocked
                        };
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid password for {loginModel.Username}. Failed attempts: {lockoutResult.FailedCount}");
                        var remainingAttempts = 3 - lockoutResult.FailedCount;
                        return new UserResponse<LoginOtpResponse>
                        {
                            IsSuccess = false,
                            StatusCode = 401,
                            Message = $"Invalid password. You have {remainingAttempts} attempt(s) remaining before your account is locked.",
                            InternalCode = ApiCode.InvalidLogin
                        };
                    }
                }

                // Password is correct - reset failed count and clear any lockout
                await _userRepository.HandleSuccessfulLoginAsync(user.Id, connection, transaction);
                _logger.LogInformation($"Successful password verification for {loginModel.Username}");

                if (user.TwoFactorEnabled)
                {
                    var token = GenerateOtpToken();
                    await _userRepository.SetAuthenticationTokenAsync(user.Id, "Email", "OTP", token, connection, transaction);

                    transaction.Commit();

                    return new UserResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse
                        {
                            User = user,
                            Token = token,
                            IsTwoFactorEnable = true
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = $"OTP sent to the email {user.Email}",
                        InternalCode = ApiCode.Success
                    };
                }
                else
                {
                    transaction.Commit();
                    return new UserResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse
                        {
                            User = user,
                            Token = string.Empty,
                            IsTwoFactorEnable = false
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "2FA is not enabled",
                        InternalCode = ApiCode.Disable2FA
                    };
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error during OTP login process");
                throw;
            }
        }

        #region Private Methods
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            var expirationTime = DateTime.Now.AddMinutes(tokenValidityInMinutes);

            try
            {
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: expirationTime,
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating JWT: {ex.Message}");
                throw;
            }
        }

        private string GenerateOtpToken()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string GenerateEmailConfirmationToken(ApplicationUser user)
        {
            var tokenData = $"{user.Id}_{user.Email}_{DateTime.UtcNow.Ticks}";
            var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes);
        }
        #endregion
    }
}



   