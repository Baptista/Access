using Access.Constants;
using Access.Data;
using Access.DataAccess;
using Access.Models;
using Access.Models.Authentication;
using Access.Services.Authentication;
using Access.Services.Email;
using Access.Services.SecureLog;
using Access.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Access.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IUserManagement _userManagement;
        private readonly ISecurityLogService _securityLogService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AuthenticationController(
            IUserRepository userRepository,
            IEmailService emailService,
            IUserManagement userManagement,
            ISecurityLogService securityLogService,
            IAuthenticationService authenticationService,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger,
            HttpClient httpClient)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _userManagement = userManagement;
            _securityLogService = securityLogService;
            _authenticationService = authenticationService;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpGet]
        [Route("Version")]
        public string Version()
        {
            return "0.0.0.1"; // Updated version for ADO.NET
        }

        // File: Access/Controllers/AuthenticationController.cs
        // Just add try-catch blocks to your existing methods

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid registration attempt from {IP}", ipAddress);
                    await _securityLogService.LogSecurityEvent(ipAddress, registerUser.Email, "Register", "Invalid Input Data");
                    return BadRequest(new Response { IsSuccess = false, Message = "Invalid input data", Status = ApiCode.InvalidInputData });
                }

#if !DEMO
        if (!registerUser.Email.EndsWith(_configuration["ClientDomain:Domain"], StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Email from invalid domain: {Email}, IP: {IP}", registerUser.Email, ipAddress);
            await _securityLogService.LogSecurityEvent(ipAddress, registerUser.Email, "Register", "Domain Invalid");
            return BadRequest(new Response { IsSuccess = false, Message = "Invalid email", Status = ApiCode.InvalidInputData });
        }
#endif

                // Check if user exists
                var existingUser = await _userRepository.GetUserByEmailAsync(registerUser.Email);
                if (existingUser != null)
                {
                    if (!existingUser.EmailConfirmed)
                    {
                        // Resend confirmation email
                        var token = await GenerateEmailConfirmationToken(existingUser);
                        var clientResetLink = $"{_configuration["ClientApp:BaseUrl"]}/ConfirmEmail?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(registerUser.Email)}";

                        var message = new Message(
                            registerUser.Email!,
                            "Resend Confirm Email",
                            $"Please confirm your email by clicking on the following link: {clientResetLink}"
                        );

                        var result = await _emailService.SendEmailAsync(message);

                        if (result.Success)
                        {
                            await _securityLogService.LogSecurityEvent(ipAddress, registerUser.Email, "Register", "Email registered but not confirmed");
                            return Ok(new Response
                            {
                                IsSuccess = true,
                                Message = "Your email is already registered but not confirmed. A new confirmation email has been sent.",
                                Status = ApiCode.EmailNotConfirmed
                            });
                        }

                        _logger.LogError("Failed to resend confirmation email to {Email}", registerUser.Email);
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { IsSuccess = false, Message = "Failed to send confirmation email. Please try again later.", Status = ApiCode.FailSendEmail });
                    }
                    await _securityLogService.LogSecurityEvent(ipAddress, registerUser.Email, "Register", "User already exists and email is confirmed");

                    return BadRequest(new Response { IsSuccess = false, Message = "User already exists and email is confirmed.", Status = ApiCode.UserAlreadyExist });
                }

                // Create new user
                var createUserResult = await _userManagement.CreateUserWithTokenAsync(registerUser);
                if (!createUserResult.IsSuccess)
                {
                    await _securityLogService.LogSecurityEvent(ipAddress, registerUser.Email, "Register", createUserResult.Message!);

                    return StatusCode(createUserResult.StatusCode, new Response
                    {
                        IsSuccess = false,
                        Message = createUserResult.Message!,
                        Status = createUserResult.InternalCode
                    });
                }

                // Send confirmation email
                var newUser = createUserResult.Response?.User;
                var confirmToken = createUserResult.Response?.Token;

                var clientResetLinkNewUser = $"{_configuration["ClientApp:BaseUrl"]}/ConfirmEmail?token={Uri.EscapeDataString(confirmToken)}&email={Uri.EscapeDataString(registerUser.Email)}";

                var messageNewUser = new Message(
                    registerUser.Email!,
                    "Confirm Email",
                    $"Please confirm your email by clicking on the following link: {clientResetLinkNewUser}"
                );

                var sendResult = await _emailService.SendEmailAsync(messageNewUser);

                if (sendResult.Success)
                {
                    _logger.LogInformation("User {Email} registered successfully", registerUser.Email);
                    return Ok(new Response { IsSuccess = true, Message = "User registered successfully. Please confirm your email.", Status = ApiCode.Success });
                }

                _logger.LogError("Failed to send confirmation email to {Email}", registerUser.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { IsSuccess = false, Message = "Failed to send confirmation email. Please try again later.", Status = ApiCode.FailSendEmail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}, IP: {IP}", registerUser.Email, ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, registerUser.Email ?? "Unknown", "Register", "System Error");
                return StatusCode(500, new Response { IsSuccess = false, Message = "An unexpected error occurred", Status = ApiCode.Error });
            }
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(ConfirmModel confirmModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            try
            {
                var result = await _authenticationService.ConfirmEmailAsync(confirmModel.email, confirmModel.token);
                if (result)
                {
                    _logger.LogInformation("Email confirmed for user {Email}", confirmModel.email);
                    return Ok(new Response { Status = ApiCode.Success, IsSuccess = true, Message = "Email verified successfully" });
                }

                _logger.LogWarning("Email confirmation failed for {Email}. Invalid or expired token", confirmModel.email);
                await _securityLogService.LogSecurityEvent(ipAddress, confirmModel.email, "ConfirmEmail", "Invalid or expired token");
                return BadRequest(new Response { Status = ApiCode.ExpiredTokenEmail, IsSuccess = false, Message = "Invalid or expired token." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during email confirmation for {Email}, IP: {IP}", confirmModel.email, ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, confirmModel.email ?? "Unknown", "ConfirmEmail", "System Error");
                return StatusCode(500, new Response { IsSuccess = false, Message = "An unexpected error occurred", Status = ApiCode.Error });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            try
            {
                if (!ModelState.IsValid)
                {
                    await _securityLogService.LogSecurityEvent(ipAddress, loginModel.Username, "Login", "Invalid Input Data");
                    return BadRequest(new Response { IsSuccess = false, Message = "Invalid input data", Status = ApiCode.InvalidInputData });
                }

                var subscription = await _userManagement.CheckSubscription(loginModel.Username);
                if (!subscription.IsSuccess)
                {
                    if (subscription.InternalCode == ApiCode.UserNotFound)
                    {
                        await _securityLogService.LogSecurityEvent(ipAddress, loginModel.Username, "Login", "User Not Found");
                        return Unauthorized(new Response { IsSuccess = false, Message = "User not found", Status = ApiCode.UserNotFound });
                    }
                    if (subscription.InternalCode == ApiCode.Error)
                    {
                        return StatusCode(subscription.StatusCode, new Response { IsSuccess = false, Message = subscription.Message, Status = subscription.InternalCode });
                    }
                    if (subscription.InternalCode == ApiCode.SubscriptionExpired)
                    {
                        _logger.LogWarning("Subscription expired for {Username}", loginModel.Username);
                        return Unauthorized(new Response { IsSuccess = false, Message = "Subscription expired", Status = ApiCode.SubscriptionExpired });
                    }
                }

                var loginOtpResponse = await _userManagement.GetOtpByLoginAsync(loginModel);

                if (!loginOtpResponse.IsSuccess)
                {
                    if (loginOtpResponse.InternalCode == ApiCode.EmailNotConfirmed)
                    {
                        var loginuser = await _userRepository.GetUserByUserNameAsync(loginModel.Username);
                        var tokenNewUser = await GenerateEmailConfirmationToken(loginuser);
                        var confirmationLinkNewUser = Url.Action(nameof(ConfirmEmail), "Authentication",
                            new { token = tokenNewUser, email = loginuser.Email }, Request.Scheme);
                        var messageNewUser = new Message(loginuser.Email!, "Confirm your email", confirmationLinkNewUser!);
                        var sendResult = await _emailService.SendEmailAsync(messageNewUser);
                    }
                    await _securityLogService.LogSecurityEvent(ipAddress, loginModel.Username, "Login", loginOtpResponse.Message!);

                    return StatusCode(loginOtpResponse.StatusCode, new Response
                    {
                        IsSuccess = false,
                        Message = loginOtpResponse.Message!,
                        Status = loginOtpResponse.InternalCode
                    });
                }

                var user = loginOtpResponse.Response?.User!;
                if (user.TwoFactorEnabled)
                {
                    var token = loginOtpResponse.Response.Token;
                    var message = new Message(user.Email!, "OTP Confirmation", token);
                    var emailResult = await _emailService.SendEmailAsync(message);

                    if (!emailResult.Success)
                    {
                        _logger.LogError("Failed to send OTP email to {Email}", user.Email);
                        return StatusCode(StatusCodes.Status500InternalServerError,
                                          new Response { IsSuccess = false, Message = "Failed to send OTP email. Please try again later.", Status = ApiCode.FailSendEmail });
                    }

                    _logger.LogInformation("OTP sent to {Email}", user.Email);
                    return Ok(new Response { IsSuccess = loginOtpResponse.IsSuccess, Status = ApiCode.Success, Message = $"We have sent an OTP to your email {user.Email}" });
                }
                else
                {
                    _logger.LogWarning("Two Factor disabled for {Username}", loginModel.Username);
                    return Unauthorized(new Response { IsSuccess = false, Message = "Two Factor disabled", Status = ApiCode.InvalidCredentials });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Username}, IP: {IP}", loginModel.Username, ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, loginModel.Username ?? "Unknown", "Login", "System Error");
                return StatusCode(500, new Response { IsSuccess = false, Message = "An unexpected error occurred", Status = ApiCode.Error });
            }
        }

        [HttpPost]
        [Route("Login-2FA")]
        public async Task<IActionResult> LoginWithOTP([FromBody] LoginWithOtpModel loginWithOtpModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            try
            {
                var jwt = await _userManagement.LoginUserWithJWTokenAsync(loginWithOtpModel.Code, loginWithOtpModel.UserName);
                if (jwt.IsSuccess)
                {
                    _logger.LogInformation("User {Username} successfully logged in with OTP", loginWithOtpModel.UserName);
                    return Ok(new Response { Result = jwt.Response, IsSuccess = true, Message = "Logged in successfully.", Status = ApiCode.Success });
                }

                _logger.LogWarning("Invalid OTP attempt for {Username}", loginWithOtpModel.UserName);
                await _securityLogService.LogSecurityEvent(ipAddress, loginWithOtpModel.UserName, "LoginWithOTP", "Invalid code");
                return Unauthorized(new Response { IsSuccess = false, Message = "Invalid code", Status = ApiCode.InvalidOTP });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OTP login for {Username}, IP: {IP}", loginWithOtpModel.UserName, ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, loginWithOtpModel.UserName ?? "Unknown", "LoginWithOTP", "System Error");
                return StatusCode(500, new Response { IsSuccess = false, Message = "An unexpected error occurred", Status = ApiCode.Error });
            }
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel forgotPasswordModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            try
            {
                if (string.IsNullOrEmpty(forgotPasswordModel.Email))
                {
                    _logger.LogWarning("Forgot password attempt with missing email from IP: {IP}", ipAddress);
                    await _securityLogService.LogSecurityEvent(ipAddress, forgotPasswordModel.Email, "ForgotPassword", "Email is required");
                    return BadRequest(new Response { IsSuccess = false, Message = "Email is required", Status = ApiCode.EmailRequired });
                }

#if !DEMO
        if (!forgotPasswordModel.Email.EndsWith(_configuration["ClientDomain:Domain"], StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Forgot password - email from invalid domain: {Email}", forgotPasswordModel.Email);
            await _securityLogService.LogSecurityEvent(ipAddress, forgotPasswordModel.Email, "ForgotPassword", "Invalid domain");
            return BadRequest(new Response { IsSuccess = false, Message = "Invalid email", Status = ApiCode.InvalidInputData });
        }
#endif

                var user = await _userRepository.GetUserByEmailAsync(forgotPasswordModel.Email);
                if (user == null || !user.EmailConfirmed)
                {
                    _logger.LogInformation("Password reset request for unconfirmed or non-existent user {Email}", forgotPasswordModel.Email);
                    await _securityLogService.LogSecurityEvent(ipAddress, forgotPasswordModel.Email, "ForgotPassword", "Unconfirmed or non-existent email");
                    return Ok(new Response { IsSuccess = true, Message = "If your email is registered and confirmed, you will receive a reset link.", Status = ApiCode.Success });
                }

                var resetToken = await _authenticationService.GeneratePasswordResetTokenAsync(user);
                var clientResetLink = $"{_configuration["ClientApp:BaseUrl"]}/ResetPassword?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(forgotPasswordModel.Email)}";

                var message = new Message(
                    forgotPasswordModel.Email!,
                    "Password Reset Request",
                    $"Please reset your password by clicking this link: {clientResetLink}"
                );
                await _emailService.SendEmailAsync(message);

                _logger.LogInformation("Password reset link sent to {Email}", forgotPasswordModel.Email);
                return Ok(new Response { IsSuccess = true, Message = "If your email is registered and confirmed, you will receive a reset link.", Status = ApiCode.Success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during forgot password for {Email}, IP: {IP}", forgotPasswordModel.Email, ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, forgotPasswordModel.Email ?? "Unknown", "ForgotPassword", "System Error");
                return StatusCode(500, new Response { IsSuccess = false, Message = "An unexpected error occurred", Status = ApiCode.Error });
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid reset password attempt from IP: {IP}", ipAddress);
                    await _securityLogService.LogSecurityEvent(ipAddress, resetPasswordModel.Email, "ResetPassword", "Invalid Input Data");
                    return BadRequest(new Response { IsSuccess = false, Message = "Invalid input data", Status = ApiCode.InvalidInputData });
                }

#if !DEMO
        if (!resetPasswordModel.Email.EndsWith(_configuration["ClientDomain:Domain"], StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Reset password - email from invalid domain: {Email}", resetPasswordModel.Email);
            await _securityLogService.LogSecurityEvent(ipAddress, resetPasswordModel.Email, "ResetPassword", "Invalid domain");
            return BadRequest(new Response { IsSuccess = false, Message = "Invalid email", Status = ApiCode.InvalidInputData });
        }
#endif

                var result = await _authenticationService.ResetPasswordAsync(resetPasswordModel.Email, resetPasswordModel.Token, resetPasswordModel.NewPassword);
                if (!result)
                {
                    _logger.LogWarning("Failed password reset for {Email}", resetPasswordModel.Email);
                    return BadRequest(new Response { IsSuccess = false, Message = "Error resetting password", Status = ApiCode.FailedResetPassword });
                }

                _logger.LogInformation("Password reset successfully for {Email}", resetPasswordModel.Email);
                return Ok(new Response { IsSuccess = true, Message = "Password has been reset successfully.", Status = ApiCode.Success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password reset for {Email}, IP: {IP}", resetPasswordModel.Email, ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, resetPasswordModel.Email ?? "Unknown", "ResetPassword", "System Error");
                return StatusCode(500, new Response { IsSuccess = false, Message = "An unexpected error occurred", Status = ApiCode.Error });
            }
        }

        [HttpGet]
        [Route("ValidateToken")]
        public async Task<IActionResult> ValidateToken()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var authHeader = Request.Headers["Authorization"].ToString();

            try
            {
                if (string.IsNullOrEmpty(authHeader))
                {
                    await _securityLogService.LogSecurityEvent(ipAddress, "Unknown", "ValidateToken", "Token missing");
                    return Unauthorized(new Response { IsSuccess = false, Message = "Token missing", Status = ApiCode.TokenMissing });
                }

                var token = authHeader.Replace("Bearer ", string.Empty);

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidAudience = _configuration["JWT:ValidAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return Ok(new Response { IsSuccess = true, Message = "Token is valid", Status = ApiCode.Success });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid token validation attempt from IP: {IP}", ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, "Unknown", "ValidateToken", "Token invalid");
                return Unauthorized(new Response { IsSuccess = false, Message = "Invalid token", Status = ApiCode.TokenInvalid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation from IP: {IP}", ipAddress);
                await _securityLogService.LogSecurityEvent(ipAddress, "Unknown", "ValidateToken", "System Error");
                return StatusCode(500, new Response { IsSuccess = false, Message = "An unexpected error occurred", Status = ApiCode.Error });
            }
        }

        // Use secure token generation
        private async Task<string> GenerateEmailConfirmationToken(ApplicationUser user)
        {
            var tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            var token = Convert.ToBase64String(tokenBytes);

            // Store with expiration
            var tokenExpiry = DateTime.UtcNow.AddMinutes(5);
            await _userRepository.SetAuthenticationTokenAsync(
                user.Id,
                "EmailConfirm",
                "Token",
                $"{token}:{tokenExpiry.Ticks}"
            );
            return token;
        }
    }
}