using ClientAcess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ClientAcess.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {           
            // Check if JWT token exists in cookies
            Request.Cookies.TryGetValue("jwtToken", out var token);
            if (string.IsNullOrEmpty(token))
            {
                // If token is missing, redirect to login
                return RedirectToAction("Login", "Account");
            }

            // Token exists, allow access
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
