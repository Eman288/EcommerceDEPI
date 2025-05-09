using EcommerceDEPI.Data;
using EcommerceDEPI.Models;
using EcommerceDEPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceDEPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Shipment)
                .Include(o => o.Payment)
                .Include(o => o.OrderDiscounts).ThenInclude(od => od.Discount)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Shipment)
                .Include(o => o.Payment)
                .Include(o => o.OrderDiscounts).ThenInclude(od => od.Discount)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task CreateOrderAsync(string userId, string address, string zipcode, string discountCode, string paymentMethod, string cardNumber = null, string expirationDate = null, string cvv = null)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(address))
                throw new InvalidOperationException("Shipping address is required.");
            if (string.IsNullOrWhiteSpace(zipcode) || !System.Text.RegularExpressions.Regex.IsMatch(zipcode, @"^\d{5}$"))
                throw new InvalidOperationException("Invalid zipcode. It must be 5 digits.");
            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new InvalidOperationException("Payment method is required.");

            var cart = await _context.Carts
                .Include(c => c.CartProducts).ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartProducts.Any())
                throw new InvalidOperationException("Your cart is empty.");

            // Check stock availability
            foreach (var cp in cart.CartProducts)
            {
                if (cp.Product == null || cp.Quantity > cp.Product.Amount)
                    throw new InvalidOperationException($"Product {cp.Product?.Name ?? "Unknown"} is out of stock.");
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                TotalPrice = cart.CartProducts.Sum(cp => cp.Quantity * cp.Product.Price),
                OrderItems = cart.CartProducts.Select(cp => new OrderItem
                {
                    ProductId = cp.ProductId,
                    Amount = cp.Quantity
                }).ToList(),
                OrderStatus = "Pending"
            };

            // Apply discount if provided
            if (!string.IsNullOrEmpty(discountCode))
            {
                var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Code == discountCode);
                if (discount != null)
                {
                    order.TotalPrice *= (1 - discount.Percentage / 100);
                    order.OrderDiscounts = new List<OrderDiscount> { new OrderDiscount { Discount = discount } };
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create shipment
            var shipment = new Shipment
            {
                OrderId = order.Id,
                Address = address,
                Zipcode = zipcode
            };
            _context.Shipments.Add(shipment);

            // Create payment
            var payment = new Payment
            {
                UserId = userId,
                OrderId = order.Id,
                Method = paymentMethod ?? "Unknown",
                Amount = order.TotalPrice,
                Date = DateTime.UtcNow
            };

            // Store partial card details only if payment method is Card and cardNumber is provided
            if (paymentMethod == "Card" && !string.IsNullOrEmpty(cardNumber))
            {
                payment.CardLastFour = cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : null;
                payment.CardType = GetCardType(cardNumber);
            }
            else
            {
                payment.CardLastFour = null;
                payment.CardType = null;
            }

            _context.Payments.Add(payment);

            // Update product stock
            foreach (var cp in cart.CartProducts)
            {
                cp.Product.Amount -= cp.Quantity;
            }

            // Clear cart
            _context.CartProducts.RemoveRange(cart.CartProducts);
            _context.Carts.Remove(cart);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to save order: {ex.Message}. Inner: {ex.InnerException?.Message ?? "No inner details"}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save order: {ex.Message}. Inner: {ex.InnerException?.Message ?? "No inner details"}", ex);
            }
        }

        public async Task<(bool success, decimal newTotal, string message)> ApplyDiscountAsync(string userId, string discountCode)
        {
            var cart = await _context.Carts
                .Include(c => c.CartProducts).ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartProducts.Any())
                return (false, 0, "Cart is empty.");

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Code == discountCode);
            if (discount == null)
                return (false, 0, "Invalid discount code.");

            var total = cart.CartProducts.Sum(cp => cp.Quantity * cp.Product.Price);
            var newTotal = total * (1 - discount.Percentage / 100);

            return (true, newTotal, "Discount applied successfully.");
        }

        public async Task AddReviewAsync(Review review, string userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == review.OrderId && o.UserId == userId);

            if (order == null || order.OrderStatus != "Delivered")
                throw new ArgumentException("Cannot add review for this order.");

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        private string GetCardType(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return "Unknown";
            if (cardNumber.StartsWith("4")) return "Visa";
            if (cardNumber.StartsWith("5")) return "Mastercard";
            if (cardNumber.StartsWith("3")) return "Amex";
            return "Unknown";
        }
    }
}