using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using ClientAccessTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;

namespace ClientAccessTemplate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://accessapi.keydevteam.com/api/authentication/");
            _logger = logger;
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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
