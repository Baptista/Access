﻿using Access.Data;
using Access.Models.Authentication;
using Access.Models;
using Access.Services.Email;
using Access.Services.User;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Response = Access.Models.Response;
using Access.Constants;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.ComponentModel;
using Polly;
using Microsoft.EntityFrameworkCore;

namespace Access.Controllers
{
    [Route("api/[controller]")]
    [ApiController]    
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IUserManagement _user;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public AuthenticationController(UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IUserManagement user,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger,
            DataContext context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _user = user;
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("migrate")]
        public IActionResult ApplyMigrations()
        {
            try
            {
                _context.Database.Migrate();
                return Ok("Migrations applied successfully.");
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return Ok("Error migrate");
            }
        }

        [HttpGet]
        [Route("Version")]
        public string Version()
        {
            return "1.0.0.0";
        }
        //[HttpGet]
        //[Route("Teste")]
        //public string Teste()
        //{
        //    try
        //    {
        //        var a = _userManager.FindByEmailAsync("bruno_baptista86@hotmail.com");
        //        return a.Result.UserName;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return "";
        //    }
        //}


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration attempt.");
                return BadRequest(new Response { IsSuccess = false, Message = "Invalid input data", Status = ApiCode.InvalidInputData });
            }
            if(!registerUser.Email.EndsWith("@keydevteam.com", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Email it´s not from keydevteam.com");
                return BadRequest(new Response { IsSuccess = false, Message = "Invalid email", Status = ApiCode.InvalidInputData });
            }
            
            // Check if the user already exists by email
            var existingUser = await _userManager.FindByEmailAsync(registerUser.Email);
            if (existingUser != null)
            {
                // Check if the email has already been confirmed
                if (!await _userManager.IsEmailConfirmedAsync(existingUser))
                {
                    // Resend confirmation email since the user exists but hasn't confirmed their email
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(existingUser);
                                        
                    var clientResetLink = $"{_configuration["ClientApp:BaseUrl"]}/ConfirmEmail?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(registerUser.Email)}";

                    var message = new Message(
                        registerUser.Email! ,
                        "Resend Confirm Email",
                        $"Please confirm your email by clicking on the following link: {clientResetLink}"
                    );

                    var result = await _emailService.SendEmailAsync(message);

                    if (result.Success)
                    {
                        return Ok(new Response
                        {
                            IsSuccess = true,
                            Message = "Your email is already registered but not confirmed. A new confirmation email has been sent.",
                            Status = ApiCode.EmailNotConfirmed
                        });
                    }

                    _logger.LogError($"Failed to resend confirmation email to {registerUser.Email}");
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { IsSuccess = false, Message = "Failed to send confirmation email. Please try again later.", Status = ApiCode.FailSendEmail });
                }

                // If the email is confirmed, return a message indicating that the user already exists
                return BadRequest(new Response { IsSuccess = false, Message = "User already exists and email is confirmed." , Status = ApiCode.UserAlreadyExist });
            }

            // Proceed with new user registration since the email is not in use
            ApplicationUser user = new ApplicationUser
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username,
                TwoFactorEnabled = true
            };

            var resultCreate = await _userManager.CreateAsync(user, registerUser.Password);
            if (!resultCreate.Succeeded)
            {
                _logger.LogError($"Failed to create user: {string.Join(", ", resultCreate.Errors.Select(e => e.Description))}");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { IsSuccess = false, Message = "User creation failed.", Status = ApiCode.UserCreationFailed });
            }

            // Generate confirmation token for new user
            var tokenNewUser = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var clientResetLinkNewUser = $"{_configuration["ClientApp:BaseUrl"]}/ConfirmEmail?token={Uri.EscapeDataString(tokenNewUser)}&email={Uri.EscapeDataString(registerUser.Email)}";

            var messageNewUser = new Message(
                registerUser.Email!,
                "Confirm Email",
                $"Please confirm your email by clicking on the following link: {clientResetLinkNewUser}"
            );
                        
            var sendResult = await _emailService.SendEmailAsync(messageNewUser);

            if (sendResult.Success)
            {
                return Ok(new Response { IsSuccess = true, Message = "User registered successfully. Please confirm your email.", Status = ApiCode.Success });
            }

            _logger.LogError($"Failed to send confirmation email to {registerUser.Email}");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { IsSuccess = false, Message = "Failed to send confirmation email. Please try again later.", Status = ApiCode.FailSendEmail });
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(ConfirmModel confirmModel)
        {
            var user = await _userManager.FindByEmailAsync(confirmModel.email);
            if (user == null)
            {
                _logger.LogWarning($"Email confirmation failed. User {confirmModel.email} does not exist.");
                return NotFound(new Response { Status = ApiCode.UserNotFound, IsSuccess = false, Message = "This user does not exist!" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmModel.token);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Email confirmed for user {confirmModel.email}.");
                return Ok(new Response { Status = ApiCode.Success, IsSuccess = true, Message = "Email verified successfully" });
            }

            _logger.LogWarning($"Email confirmation failed for {confirmModel.email}. Invalid or expired token.");
            return BadRequest(new Response { Status = ApiCode.ExpiredTokenEmail, IsSuccess = false, Message = "Invalid or expired token." });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt.");
                return BadRequest(new Response { IsSuccess = false, Message = "Invalid input data" , Status = ApiCode.InvalidInputData});
            }

            var loginOtpResponse = await _user.GetOtpByLoginAsync(loginModel);
            if (!loginOtpResponse.IsSuccess)
            {
                if(loginOtpResponse.InternalCode == ApiCode.EmailNotConfirmed)
                {
                    var loginuser = await _userManager.FindByNameAsync(loginModel.Username);
                    // Generate confirmation token for new user
                    var tokenNewUser = await _userManager.GenerateEmailConfirmationTokenAsync(loginuser);

                    // Create confirmation link
                    var confirmationLinkNewUser = Url.Action(nameof(ConfirmEmail), "Authentication",
                        new { token = tokenNewUser, email = loginuser.Email }, Request.Scheme);

                    // Send confirmation email
                    var messageNewUser = new Message(loginuser.Email , "Confirm your email", confirmationLinkNewUser);
                    var sendResult = await _emailService.SendEmailAsync(messageNewUser);
                }

                return StatusCode(loginOtpResponse.StatusCode, new Response
                {
                    IsSuccess = false,
                    Message = loginOtpResponse.Message,
                    Status = loginOtpResponse.InternalCode
                });
            }

            var user = loginOtpResponse.Response.User;
            if (user.TwoFactorEnabled)
            {
                var token = loginOtpResponse.Response.Token;
                var message = new Message(user.Email!, "OTP Confirmation", token);
                var emailResult = await _emailService.SendEmailAsync(message);

                if (!emailResult.Success)
                {
                    _logger.LogError($"Failed to send OTP email to {user.Email}.");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                                      new Response { IsSuccess = false, Message = "Failed to send OTP email. Please try again later.", Status = ApiCode.FailSendEmail });
                }

                _logger.LogInformation($"OTP sent to {user.Email}.");
                return Ok(new Response { IsSuccess = loginOtpResponse.IsSuccess, Status = ApiCode.Success, Message = $"We have sent an OTP to your email {user.Email}" });
            }
            else
            {
                _logger.LogWarning($"Two Factor disabled {loginModel.Username}.");
                return Unauthorized(new Response { IsSuccess = false, Message = "Two Factor disabled", Status = ApiCode.InvalidCredentials });
            }            
        }

        [HttpPost]
        [Route("Login-2FA")]
        public async Task<IActionResult> LoginWithOTP([FromBody] LoginWithOtpModel loginWithOtpModel)
        {
            var jwt = await _user.LoginUserWithJWTokenAsync(loginWithOtpModel.Code, loginWithOtpModel.UserName);
            if (jwt.IsSuccess)
            {
                _logger.LogInformation($"User {loginWithOtpModel.UserName} successfully logged in with OTP.");
                return Ok(new Response { Result = jwt.Response, IsSuccess = true, Message = "Logged in successfully.", Status = ApiCode.Success });
            }

            _logger.LogWarning($"Invalid OTP attempt for {loginWithOtpModel.UserName}.");
            return Unauthorized(new Response { IsSuccess = false, Message = "Invalid code" , Status = ApiCode.InvalidOTP });
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel forgotPasswordModel)
        {
            if (string.IsNullOrEmpty(forgotPasswordModel.Email))
            {
                _logger.LogWarning("Forgot password attempt with missing email.");
                return BadRequest(new Response { IsSuccess = false, Message = "Email is required", Status = ApiCode.EmailRequired  });
            }
            if (!forgotPasswordModel.Email.EndsWith("@keydevteam.com", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Email it´s not from keydevteam.com");
                return BadRequest(new Response { IsSuccess = false, Message = "Invalid email", Status = ApiCode.InvalidInputData });
            }
            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                _logger.LogInformation($"Password reset request for unconfirmed or non-existent user {forgotPasswordModel.Email}.");
                return Ok(new Response { IsSuccess = true, Message = "If your email is registered and confirmed, you will receive a reset link.", Status = ApiCode.Success });
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            // **Update the Reset Link to Point to the Client Application**
            var clientResetLink = $"{_configuration["ClientApp:BaseUrl"]}/ResetPassword?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(forgotPasswordModel.Email)}";

            var message = new Message(
                forgotPasswordModel.Email! ,
                "Password Reset Request",
                $"Please reset your password by clicking this link: {clientResetLink}"
            );
            await _emailService.SendEmailAsync(message);

            _logger.LogInformation($"Password reset link sent to {forgotPasswordModel.Email}.");
            return Ok(new Response { IsSuccess = true, Message = "If your email is registered and confirmed, you will receive a reset link.", Status = ApiCode.Success });
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reset password attempt.");
                return BadRequest(new Response { IsSuccess = false, Message = "Invalid input data", Status = ApiCode.InvalidInputData});
            }
            if (!resetPasswordModel.Email.EndsWith("@keydevteam.com", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Email it´s not from keydevteam.com");
                return BadRequest(new Response { IsSuccess = false, Message = "Invalid email", Status = ApiCode.InvalidInputData });
            }
            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
            {
                _logger.LogWarning($"Reset password attempt for non-existent email {resetPasswordModel.Email}.");
                return BadRequest(new Response { IsSuccess = false, Message = "Invalid email", Status = ApiCode.EmailNotFound });
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.NewPassword);
            if (!resetPassResult.Succeeded)
            {
                var errors = string.Join(", ", resetPassResult.Errors.Select(e => e.Description));
                _logger.LogWarning($"Failed password reset for {resetPasswordModel.Email}. Errors: {errors}");
                return BadRequest(new Response { IsSuccess = false, Message = $"Error resetting password: {errors}" , Status = ApiCode.FailedResetPassword });
            }

            _logger.LogInformation($"Password reset successfully for {resetPasswordModel.Email}.");
            return Ok(new Response { IsSuccess = true, Message = "Password has been reset successfully.", Status = ApiCode.Success });
        }

        //[HttpPost]
        //[Route("Refresh-Token")]
        //public async Task<IActionResult> RefreshToken(LoginResponse tokens)
        //{
        //    var jwt = await _user.RenewAccessTokenAsync(tokens);
        //    if (jwt.IsSuccess)
        //    {
        //        _logger.LogInformation($"Token refreshed successfully for {tokens.Username}.");
        //        return Ok(new Response { Result = jwt, IsSuccess = true, Message = "Token refreshed successfully.", Status = ApiCode.Success });
        //    }

        //    _logger.LogWarning($"Failed token refresh attempt for {tokens.Username}.");
        //    return Unauthorized(new Response { Status = ApiCode.RefreshTokenExpired, IsSuccess = false, Message = "Invalid token or refresh token." });
        //}


        [HttpGet]
        [Route("ValidateToken")]
        public async Task<IActionResult> ValidateToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized(new Response { IsSuccess = false, Message = "Token missing", Status = ApiCode.TokenMissing });
            }

            var token = authHeader.Replace("Bearer ", string.Empty);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidAudience = _configuration["JWT:ValidAudience"],
                    ValidateLifetime = true, // Enforce expiration check
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return Ok(new Response { IsSuccess = true, Message = "Token is valid", Status = ApiCode.Success });
            }
            catch (Exception ex)
            {
                return Unauthorized(new Response { IsSuccess = false, Message = ex.Message, Status = ApiCode.TokenInvalid });
            }
        }


    }
}
