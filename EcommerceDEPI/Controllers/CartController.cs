using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using EcommerceDEPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EcommerceDEPI.Data;

namespace EcommerceDEPI.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CartController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            if (product.Amount <= 0)
            {
                return Json(new { success = false, message = "Product is out of stock" });
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartProduct = await _context.CartProducts.FirstOrDefaultAsync(cp => cp.CartId == cart.Id && cp.ProductId == product.Id);

            if (cartProduct == null)
            {
                cartProduct = new CartProduct
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = 1
                };
                _context.CartProducts.Add(cartProduct);
            }
            else
            {
                if (cartProduct.Quantity + 1 > product.Amount)
                {
                    return Json(new { success = false, message = "Not enough stock available" });
                }
                cartProduct.Quantity++;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.CartProducts)
                    .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id, CartProducts = new List<CartProduct>() };
            }

            if (!cart.CartProducts.Any())
            {
                ViewBag.Message = "Your cart is empty.";
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (cart == null) return RedirectToAction("Index");

            var cartProduct = await _context.CartProducts.FirstOrDefaultAsync(cp => cp.CartId == cart.Id && cp.ProductId == productId);
            if (cartProduct != null)
            {
                _context.CartProducts.Remove(cartProduct);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("OrderConfirmation");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int change)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (cart == null) return Json(new { success = false });

            var cartProduct = await _context.CartProducts
                .Include(cp => cp.Product)
                .FirstOrDefaultAsync(cp => cp.CartId == cart.Id && cp.ProductId == productId);

            if (cartProduct != null)
            {
                cartProduct.Quantity += change;

                decimal itemTotal = 0;

                if (cartProduct.Quantity <= 0)
                {
                    _context.CartProducts.Remove(cartProduct);
                }
                else
                {
                    if (cartProduct.Quantity > cartProduct.Product.Amount)
                    {
                        return Json(new { success = false, message = "Not enough stock available" });
                    }
                    itemTotal = cartProduct.Product.Price * cartProduct.Quantity;
                }

                await _context.SaveChangesAsync();

                decimal cartTotal = await _context.CartProducts
                    .Include(cp => cp.Product)
                    .Where(cp => cp.CartId == cart.Id)
                    .SumAsync(cp => cp.Quantity * cp.Product.Price);

                return Json(new { success = true, itemTotal, cartTotal });
            }

            return Json(new { success = false });
        }
    }
}