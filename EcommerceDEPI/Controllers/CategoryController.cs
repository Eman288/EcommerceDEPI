using Microsoft.AspNetCore.Mvc;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EcommerceDEPI.Models;
using EcommerceDEPI.Data;


namespace EcommerceDEPI.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }
        //[Authorize(Roles = "Admin")]
        //[HttpGet]
        //public IActionResult Create()
        //{
        //    return View(new Category());
        //}

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection cData)
        {
            if (string.IsNullOrEmpty(cData["name"]))
            {
                TempData["CategoryError"] = "Name can't be empty";
                return RedirectToAction("Controls", "User");
            }
            var name = cData["name"].ToString();
            var oldC = await _db.Categories.FirstOrDefaultAsync(a => a.Name == name);

            if (oldC != null)
            {
                TempData["CategoryError"] = "There is already a category with this name";
                return RedirectToAction("Controls", "User");
            }

            Category g = new Category { Name = name };
            _db.Categories.Add(g);
            await _db.SaveChangesAsync();

            TempData["CategorySuccess"] = "Category created successfully";
            return RedirectToAction("Controls", "User");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int cID)
        {
            var oldC = await _db.Categories.Where(a => a.Id == cID).FirstOrDefaultAsync();
            if (oldC == null)
            {
                return RedirectToAction("Controls", "User");

            }
            _db.Categories.Remove(oldC);
            await _db.SaveChangesAsync();
            return RedirectToAction("Controls", "User");
        }


        [Authorize(Roles = "Admin")] 
        [HttpPost]
        public async Task<IActionResult> Edit(IFormCollection cData)
        {
            var id = int.Parse(cData["id"]);
            var name = cData["name"].ToString();
            var existingCategory = await _db.Categories.Where(a => a.Id == id).FirstOrDefaultAsync();

            var oldC = await _db.Categories.Where(a => a.Name == name).FirstOrDefaultAsync();
            if (oldC == null) // new name and no one got it before
            {
                // do nothing for now
            }
            else if (id != oldC.Id) // new name but already obtained by another row
            {
                TempData["CategoryErrorE"] = "There is already a category with this name";
                TempData["cid"] = id;
                return RedirectToAction("Controls", "User");
            }
            // else same name no change


            existingCategory.Name = name;
            await _db.SaveChangesAsync();
            TempData["CategorySuccessE"] = "The Category was Updated";
            return RedirectToAction("Controls", "User");
        }


    }
}
