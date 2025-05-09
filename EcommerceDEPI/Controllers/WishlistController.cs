using EcommerceDEPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using EcommerceDEPI.Data;

namespace EcommerceDEPI.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = _userManager.GetUserId(User);

            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            var exists = await _context.WishlistProducts
                .AnyAsync(wp => wp.ProductId == productId && wp.WishlistId == wishlist.Id);

            if (!exists)
            {
                var wishlistProduct = new WishlistProduct
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId
                };

                _context.WishlistProducts.Add(wishlistProduct);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Product");
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var wishlistItems = await _context.WishlistProducts
                .Include(wp => wp.Product)
                .Where(wp => wp.Wishlist.UserId == userId)
                .ToListAsync();

            return View(wishlistItems);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = _userManager.GetUserId(User);

            var item = await _context.WishlistProducts
                .Include(wp => wp.Wishlist)
                .FirstOrDefaultAsync(wp => wp.ProductId == productId && wp.Wishlist.UserId == userId);

            if (item != null)
            {
                _context.WishlistProducts.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
