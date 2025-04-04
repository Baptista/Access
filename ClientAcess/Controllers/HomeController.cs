using ClientAcess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;

namespace ClientAcess.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
#if DEBUG
            _httpClient.BaseAddress = new Uri("http://localhost:5222/api/authentication/");
#else
            _httpClient.BaseAddress = new Uri("https://accessapi.keydevteam.com/api/authentication/");
#endif
        }

        public async Task<IActionResult> Index()
        {
            Request.Cookies.TryGetValue("jwtToken", out var token);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("ValidateToken");
                if (!response.IsSuccessStatusCode)
                {
                    Response.Cookies.Append("jwtToken", "", new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(-1) // Expire the cookie
                    });
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }           
        }

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View();
        //}
    }
}
