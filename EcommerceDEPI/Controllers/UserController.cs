using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceDEPI.Models;
using Microsoft.EntityFrameworkCore;
using EcommerceDEPI.Data;

namespace EcommerceDEPI.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Controls()
        {
            var cats = await _db.Categories.ToListAsync();
            ViewData["cats"] = cats;

            var pros = await _db.Products.ToListAsync();
            ViewData["pros"] = pros;
            return View();
        }
    }
}
