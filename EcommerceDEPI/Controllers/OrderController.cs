using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceDEPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EcommerceDEPI.Services.Interfaces;
using EcommerceDEPI.Services;
using System.Text.RegularExpressions;

namespace EcommerceDEPI.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<OrderController> _logger;
        private readonly CardService _cardService;

        public OrderController(IOrderService orderService, UserManager<User> userManager, ILogger<OrderController> logger, CardService cardService)
        {
            _orderService = orderService;
            _userManager = userManager;
            _logger = logger;
            _cardService = cardService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetOrdersByUserIdAsync(user.Id);
            return View(orders);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(string address, string zipcode, string discountCode, string paymentMethod, string cardNumber, string expirationDate, string cvv)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Please login to complete your checkout.";
                return RedirectToAction("Index", "Cart");
            }

            // Validate address
            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["Error"] = "Shipping address is required.";
                return RedirectToAction("Index", "Cart");
            }

            // Validate zipcode (5 digits)
            if (string.IsNullOrWhiteSpace(zipcode) || !Regex.IsMatch(zipcode, @"^\d{5}$"))
            {
                TempData["Error"] = "Invalid zipcode. It must be 5 digits.";
                return RedirectToAction("Index", "Cart");
            }

            // Validate payment method
            if (string.IsNullOrWhiteSpace(paymentMethod) || !new[] { "Cash", "Card", "Paypal" }.Contains(paymentMethod))
            {
                TempData["Error"] = "Please select a valid payment method.";
                return RedirectToAction("Index", "Cart");
            }

            // Validate card details only if payment method is Card
            if (paymentMethod == "Card")
            {
                if (string.IsNullOrWhiteSpace(cardNumber) || string.IsNullOrWhiteSpace(cvv) || string.IsNullOrWhiteSpace(expirationDate))
                {
                    TempData["Error"] = "Please provide all card details.";
                    return RedirectToAction("Index", "Cart");
                }

                // Validate card format
                if (!Regex.IsMatch(cardNumber, @"^\d{15,16}$"))
                {
                    TempData["Error"] = "Card number must be 15 or 16 digits.";
                    return RedirectToAction("Index", "Cart");
                }
                if (!Regex.IsMatch(cvv, @"^\d{3,4}$"))
                {
                    TempData["Error"] = "CVV must be 3 or 4 digits.";
                    return RedirectToAction("Index", "Cart");
                }
                if (!Regex.IsMatch(expirationDate, @"^(0[1-9]|1[0-2])\/[0-9]{2}$"))
                {
                    TempData["Error"] = "Expiration date must be in MM/YY format.";
                    return RedirectToAction("Index", "Cart");
                }

                // Check if card exists in JSON
                bool isCardValid = await _cardService.IsCardValidAsync(cardNumber, cvv, expirationDate);
                if (!isCardValid)
                {
                    TempData["Error"] = "This card is not registered. Please use a valid card.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            else
            {
                // Clear card details if payment method is not Card
                cardNumber = null;
                cvv = null;
                expirationDate = null;
            }

            try
            {
                await _orderService.CreateOrderAsync(user.Id, address, zipcode, discountCode, paymentMethod, cardNumber, expirationDate, cvv);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during checkout for user {UserId}", user.Id);
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout for user {UserId}. Inner Exception: {InnerException}", user.Id, ex.InnerException?.Message);
                TempData["Error"] = $"An error occurred while processing your order: {ex.Message}. Inner: {ex.InnerException?.Message}";
                return RedirectToAction("Index", "Cart");
            }

            var orders = await _orderService.GetOrdersByUserIdAsync(user.Id);
            return RedirectToAction("OrderConfirmation", new { id = orders.Last().Id });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ApplyDiscount(string discountCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not logged in." });

            try
            {
                var (success, newTotal, message) = await _orderService.ApplyDiscountAsync(user.Id, discountCode);
                return Json(new { success, newTotal, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying discount for user {UserId}", user.Id);
                return Json(new { success = false, message = "An error occurred while applying the discount." });
            }
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(id, user.Id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(id, user.Id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> AddReview(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId, user.Id);
            if (order == null || order.OrderStatus != "Delivered")
            {
                return NotFound();
            }

            var review = new Review
            {
                OrderId = orderId,
                ProductId = order.OrderItems.First().ProductId,
                UserId = user.Id
            };

            return View(review);
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(Review review)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid for review submission: {@Errors}",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(review);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _orderService.AddReviewAsync(review, user.Id);
                _logger.LogInformation("Review successfully saved for OrderId: {OrderId}", review.OrderId);
                return RedirectToAction("Details", new { id = review.OrderId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error while saving review for OrderId: {OrderId}", review.OrderId);
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View(review);
            }
        }
    }
}