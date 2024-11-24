using Access.Models.Authentication;
using ClientAcess.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ClientAcess.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public AccountController(HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5222/api/Authentication/");
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            model.Roles = ["User"];
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("register", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            var apiError = JsonConvert.DeserializeObject<Response>(errorContent);

            if (apiError != null && !string.IsNullOrEmpty(apiError.Message))
            {
                ViewBag.Message = apiError.Message;                
            }
            else
            {
                ViewBag.Message = "Error registering user";                
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() {
            Request.Cookies.TryGetValue("jwtToken", out var token);
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View(); 
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("login", model);

            if (response.IsSuccessStatusCode)
            {   
                return RedirectToAction("LoginWithOTP", new { userName = model .Username});
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            var apiError = JsonConvert.DeserializeObject<Response>(errorContent); 

            if (apiError != null && !string.IsNullOrEmpty(apiError.Message))
            {
                ViewBag.Message = apiError.Message;                
            }
            else
            {
                ViewBag.Message = "Invalid login attempt";                
            }            
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PostAsJsonAsync("resetpassword", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            ViewBag.Message = "Failed to reset password";            
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PostAsJsonAsync("forgotpassword", model);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "If the email exists, a reset link has been sent.";
                return View();
            }
            ViewBag.Message = "Error sending reset link.";            
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginWithOTP(string userName)
        {
            var model = new LoginWithOtpModel { UserName = userName };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoginWithOTP(LoginWithOtpModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PostAsJsonAsync($"login-2FA", model);

            if (response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var apiError = JsonConvert.DeserializeObject<Response>(errorContent); 
                var tokentime = string.IsNullOrEmpty(_configuration["ExpirateTokenTime"])? Convert.ToDouble(_configuration["ExpirateTokenTime"]) : 24;
                Response.Cookies.Append("jwtToken", apiError.Result.Response?.AccessToken.Token, new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true,   
                    Expires = apiError.Result.Response?.AccessToken.ExpiryTokenDate.AddHours(tokentime)
                });
                                
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.Message = "Invalid OTP. Please try again.";
            return View(model);
        }
    }
}
