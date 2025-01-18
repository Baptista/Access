using Access.Models.Authentication;
using ClientAcess.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

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
#if DEBUG
            _httpClient.BaseAddress = new Uri("http://localhost:5222/api/Authentication/");
#else
            _httpClient.BaseAddress = new Uri("https://accessapi.keydevteam.com/api/authentication/");
#endif


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
                return RedirectToAction("SendEmail");                
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

        public IActionResult SendEmail()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            ConfirmModel confirmModel = new ConfirmModel();
            confirmModel.email = email;
            confirmModel.token = token;
            var response = await _httpClient.PostAsJsonAsync("ConfirmEmail",confirmModel);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var apiError = JsonConvert.DeserializeObject<Response>(errorContent);

                if (apiError != null && !string.IsNullOrEmpty(apiError.Message))
                {
                    ViewBag.Message = apiError.Message;
                }
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Login() {
            Request.Cookies.TryGetValue("jwtToken", out var token);
            if (!string.IsNullOrEmpty(token))            
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("ValidateToken");
                if (response.IsSuccessStatusCode)
                {                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Response.Cookies.Append("jwtToken", "", new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(-1) // Expire the cookie
                    });
                }
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
                Response.Cookies.Append("jwtToken", apiError.Result.AccessToken.Token, new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true  
                    
                });
                                
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Invalid OTP. Please try again.";
            return View(model);
        }        
    }
}
