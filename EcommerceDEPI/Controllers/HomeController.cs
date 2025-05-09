using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDEPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EcommerceDEPI.Services.Interfaces;
using EcommerceDEPI.Data;
using EcommerceDEPI.Data;
using EcommerceApp.Models;
using System.Diagnostics;


namespace EcommerceDEPI.Models
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //[Authorize] // put when want to make sure the user is logged in before he/she use this action
        public IActionResult Index(string search)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => string.IsNullOrEmpty(search) ||
                            p.Name.Contains(search) ||
                            p.Description.Contains(search))
                .ToList();

            return View(products);
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
