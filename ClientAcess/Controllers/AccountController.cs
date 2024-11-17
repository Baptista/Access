using Access.Models.Authentication;
using ClientAcess.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ClientAcess.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5222/api/Authentication/");
        }
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            model.Roles = [ "User"];
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("register", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            var apiError = JsonConvert.DeserializeObject<ApiResponse>(errorContent); // Adjust to your API error model

            if (apiError != null && !string.IsNullOrEmpty(apiError.Message))
            {
                ModelState.AddModelError(string.Empty, apiError.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error registering user");
            }            
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                //var authResponse = JsonConvert.DeserializeObject<AuthResponse>(result);

                // Armazenar o token na sessão ou cookie
                //HttpContext.Session.SetString("JWToken", authResponse.Token);

                return RedirectToAction("Index", "Home");
            } // Extract error message from API response
            var errorContent = await response.Content.ReadAsStringAsync();
            var apiError = JsonConvert.DeserializeObject<ApiResponse>(errorContent); // Adjust to your API error model

            if (apiError != null && !string.IsNullOrEmpty(apiError.Message))
            {
                ModelState.AddModelError(string.Empty, apiError.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
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

            ModelState.AddModelError(string.Empty, "Failed to reset password.");
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

            ModelState.AddModelError(string.Empty, "Error sending reset link.");
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
                // Handle successful login, e.g., redirect to a dashboard
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.Message = "Invalid OTP. Please try again.";
            return View(model);
        }
    }
}
