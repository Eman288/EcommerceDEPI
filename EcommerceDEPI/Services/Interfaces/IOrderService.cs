using EcommerceDEPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceDEPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order> GetOrderByIdAsync(int orderId, string userId);
        Task CreateOrderAsync(string userId, string address, string zipcode, string discountCode, string paymentMethod, string cardNumber = null, string expirationDate = null, string cvv = null);
        Task AddReviewAsync(Review review, string userId);
        Task<(bool success, decimal newTotal, string message)> ApplyDiscountAsync(string userId, string discountCode);
    }
}