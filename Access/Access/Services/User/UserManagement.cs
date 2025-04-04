﻿using Access.Data;
using Access.Models.Authentication;
using Access.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Access.Constants;
using System;

namespace Access.Services.User
{
    public class UserManagement : IUserManagement
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserManagement> _logger;
        private readonly HttpClient _httpClient;
        public UserManagement(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<UserManagement> logger,
            HttpClient httpClient)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<UserResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, ApplicationUser user)
        {
            var assignedRole = new List<string>();
            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    if (!await _userManager.IsInRoleAsync(user, role))
                    {
                        var result = await _userManager.AddToRoleAsync(user, role);
                        if (result.Succeeded)
                        {
                            assignedRole.Add(role);
                        }
                        else
                        {
                            _logger.LogError($"Failed to assign role {role} to user {user.UserName}");
                            return new UserResponse<List<string>>
                            {
                                IsSuccess = false,
                                StatusCode = 500,
                                Message = $"Failed to assign role {role}.",
                                InternalCode = ApiCode.FailedToAssignRole
                            };
                        }
                    }
                }
            }

            return new UserResponse<List<string>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Roles have been assigned",
                Response = assignedRole,
                InternalCode = ApiCode.Success
            };
        }

        public async Task<UserResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser)
        {
            // Check if the user or username already exists
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null)
            {
                return new UserResponse<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "Email already exists!", InternalCode = ApiCode.EmailAlreadyExists };
            }

            var usernameExist = await _userManager.FindByNameAsync(registerUser.Username);
            if (usernameExist != null)
            {
                return new UserResponse<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "Username already exists!" , InternalCode = ApiCode.UserAlreadyExists };
            }

            ApplicationUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username,
                TwoFactorEnabled = true
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return new UserResponse<CreateUserResponse> { IsSuccess = false, StatusCode = 500, Message = "User creation failed" , InternalCode = ApiCode.UserCreationFailed };
            }

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to generate email confirmation token.");
                return new UserResponse<CreateUserResponse> { IsSuccess = false, StatusCode = 500, Message = "Token generation failed", InternalCode = ApiCode.TokenGenerationFailed };
            }

            return new UserResponse<CreateUserResponse>
            {
                Response = new CreateUserResponse { User = user, Token = token },
                IsSuccess = true,
                StatusCode = 201,
                Message = "User created and confirmation token generated",
                InternalCode = ApiCode.Success
            };
        }

        public async Task<UserResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username);
            if (user != null)
            {

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    _logger.LogWarning($"Login failed for {loginModel.Username}. Email not confirmed.");
                    return new UserResponse<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "Email not confirmed. Please check your email and confirm your account.",
                        InternalCode= ApiCode.EmailNotConfirmed
                    };
                }
                var isValid = await _userManager.CheckPasswordAsync(user, loginModel.Password);
                if (!isValid)
                {
                    return new UserResponse<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "Invalid Password",
                        InternalCode = ApiCode.InvalidLogin
                    };
                }
                var islocked = await _userManager.IsLockedOutAsync(user);
                if (islocked)
                {
                    return new UserResponse<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User Locked",
                        InternalCode = ApiCode.InvalidLogin
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);
                if (result.IsLockedOut)
                {
                    return new UserResponse<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 423, // Locked out status code
                        Message = "User account locked due to multiple failed login attempts.",
                        InternalCode = ApiCode.UserAccountLocked
                    };
                }

                if (result.RequiresTwoFactor)
                {
                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    await _userManager.SetAuthenticationTokenAsync(user, "Email", "OTP", token); // Store the token explicitly

                    return new UserResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse
                        {
                            User = user,
                            Token = token,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = $"OTP sent to the email {user.Email}",
                        InternalCode = ApiCode.Success
                    };
                }
                else
                {
                    return new UserResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse
                        {
                            User = user,
                            Token = string.Empty,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "2FA is not enabled",
                        InternalCode = ApiCode.Disable2FA
                    };
                }
            }
            else
            {
                return new UserResponse<LoginOtpResponse>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = "User does not exist.",
                    InternalCode = ApiCode.UserNotFound
                };
            }
        }

        public async Task<UserResponse<LoginResponse>> GetJwtTokenAsync(ApplicationUser user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Generate access and refresh tokens
            var jwtToken = GetToken(authClaims); // Access token
            
            await _userManager.UpdateAsync(user);

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

        public async Task<UserResponse<LoginResponse>> LoginUserWithJWTokenAsync(string otp, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new UserResponse<LoginResponse>
                {
                    IsSuccess = false,
                    StatusCode = 401,
                    Message = "Invalid userName",
                    InternalCode = ApiCode.UserNotFound
                };
            }

            var token = await _userManager.GetAuthenticationTokenAsync(user, "Email", "OTP");
            if (string.IsNullOrEmpty(token) || token != otp)
            {
                return new UserResponse<LoginResponse>
                {
                    IsSuccess = false,
                    StatusCode = 401,
                    Message = "Invalid OTP",
                    InternalCode = ApiCode.InvalidOTP
                };
            }

            // Validate the OTP and remove it after use
            var isOtpValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", otp);
            if (isOtpValid)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "Email", "OTP"); // Invalidate OTP
                return await GetJwtTokenAsync(user);
            }
            else
            {
                return new UserResponse<LoginResponse>
                {
                    IsSuccess = false,
                    StatusCode = 401,
                    Message = "Invalid OTP",
                    InternalCode = ApiCode.InvalidOTP
                };
            }
        }

        //public async Task<UserResponse<LoginResponse>> RenewAccessTokenAsync(LoginResponse tokens)
        //{
        //    var accessToken = tokens.AccessToken;
        //    var refreshToken = tokens.RefreshToken;

        //    var principal = GetClaimsPrincipal(accessToken.Token);
        //    var user = await _userManager.FindByNameAsync(principal.Identity.Name);

        //    if (refreshToken.Token != user.RefreshToken)
        //    {
        //        return new UserResponse<LoginResponse>
        //        {
        //            IsSuccess = false,
        //            StatusCode = 400,
        //            Message = "Invalid refresh token",
        //            InternalCode = ApiCode.InvalidRefreshToken
        //        };
        //    }

        //    if (refreshToken.ExpiryTokenDate <= DateTime.Now)
        //    {
        //        return new UserResponse<LoginResponse>
        //        {
        //            IsSuccess = false,
        //            StatusCode = 400,
        //            Message = "Refresh token expired",
        //            InternalCode = ApiCode.RefreshTokenExpired
        //        };
        //    }

        //    var response = await GetJwtTokenAsync(user);
        //    return response;
        //}

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

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetClaimsPrincipal(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = true, // Enforce token expiration validation
                ClockSkew = TimeSpan.Zero  // Adjust for server time differences
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            return principal;
        }

        public async Task<UserResponse<bool>> CheckSubscription(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
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
            if (emailDomain.Length != 2) {
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
            _logger.LogWarning($"{errorContent}");
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

        #endregion
    }
}
