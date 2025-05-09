using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDEPI.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceDEPI.Data;

namespace EcommerceDEPI.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string search = "", string category = "")
        {
            var products = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.Name.ToLower().Contains(search.ToLower()) ||
                    p.Description.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(category) && category != "الكل")
            {
                products = products.Where(p => p.Category.Name == category);
            }

            ViewData["categories"] = _db.Categories.ToList();
            return View(products.ToList());
        }

        public IActionResult Details(int id)
        {
            var product = _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection pData, IFormFile? imageFile)
        {
            if (string.IsNullOrEmpty(pData["name"]))
            {
                TempData["ProductError"] = "Name Can't be empty";
                return RedirectToAction("Controls", "User");
            }


            if (string.IsNullOrEmpty(pData["description"]))
            {
                TempData["ProductError"] = "Description Can't be empty";
                return RedirectToAction("Controls", "User");
            }

            if (int.Parse(pData["amount"]) < 0)
            {
                TempData["ProductError"] = "The Product's amount can't be less than 0";
                return RedirectToAction("Controls", "User");
            }

            if (int.Parse(pData["price"]) < 0)
            {
                TempData["ProductError"] = "The Product's Price can't be less than 0";
                return RedirectToAction("Controls", "User");
            }

            var oldP = await _db.Products.Where(a => a.Name == pData["name"].ToString()).FirstOrDefaultAsync();
            if (oldP != null)
            {
                TempData["ProductError"] = "There is already a Product with this name";
                return RedirectToAction("Controls", "User");
            }

            var cat = await _db.Categories.Where(c => c.Name == pData["category"].ToString()).FirstOrDefaultAsync();

            if (cat == null)
            {
                TempData["ProductError"] = "Category Was Empty";

                return RedirectToAction("Controls", "User");
            }
            Product nP = new Product();
            nP.Name = pData["name"];
            nP.Description = pData["description"];
            nP.Price = int.Parse(pData["price"]);
            nP.Amount = int.Parse(pData["amount"]);
            nP.CategoryId = cat.Id;
            nP.Category = cat;
            nP.Picture = "wwwroot/imgs/Product/default.jpg";
            // Save the image
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imgs/Product", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

               nP.Picture = "/imgs/Product/" + fileName;
            }

            _db.Products.Add(nP);
            await _db.SaveChangesAsync();
            TempData["ProductSuccess"] = "Product was Created";
            return RedirectToAction("Controls", "User");
        }

     
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(IFormCollection pData, IFormFile? imageFile)
        {
            if (string.IsNullOrEmpty(pData["id"]))
            {
                TempData["ProductErrorE"] = "No Product Id Provided";
                return RedirectToAction("Controls", "User");
            }

            int id = int.Parse(pData["id"]);
            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ProductErrorE"] = "Product not found";
                return RedirectToAction("Controls", "User");
            }

            if (string.IsNullOrEmpty(pData["name"]))
            {
                TempData["ProductErrorE"] = "Name can't be empty";
                return RedirectToAction("Controls", "User");
            }

            if (string.IsNullOrEmpty(pData["description"]))
            {
                TempData["ProductErrorE"] = "Description can't be empty";
                return RedirectToAction("Controls", "User");
            }

            if (!int.TryParse(pData["price"], out int price) || price < 0)
            {
                TempData["ProductErrorE"] = "Invalid Price";
                return RedirectToAction("Controls", "User");
            }

            if (!int.TryParse(pData["amount"], out int amount) || amount < 0)
            {
                TempData["ProductErrorE"] = "Invalid Amount";
                return RedirectToAction("Controls", "User");
            }

            product.Name = pData["name"];
            product.Description = pData["description"];
            product.Price = price;
            product.Amount = amount;

            // Update Category
            var cat = await _db.Categories.Where(c => c.Name == pData["category"].ToString()).FirstOrDefaultAsync();
            if (cat == null)
            {
                TempData["ProductErrorE"] = "Category was Empty";
                return RedirectToAction("Controls", "User");
            }
            product.CategoryId = cat.Id;
            product.Category = cat;

            // Update Picture
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imgs/Product", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.Picture = "/imgs/Product/" + fileName;
            }

            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            TempData["ProductSuccessE"] = "Product was Updated";
            return RedirectToAction("Controls", "User");
        }
        

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int pID)
        {
            var oldP = await _db.Products.FirstOrDefaultAsync(a => a.Id == pID);
            if (oldP == null) return RedirectToAction("Controls", "User");


            _db.Products.Remove(oldP);
            await _db.SaveChangesAsync();
            return RedirectToAction("Controls", "User");
        }


    }
}
