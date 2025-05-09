using EcommerceDEPI.Models;

namespace EcommerceDEPI.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<Product> Products { get; }
        IRepository<Discount> Discounts { get; }
        IRepository<Cart> Carts { get; }
        IRepository<CartProduct> CartProducts { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Shipment> Shipments { get; }
        IRepository<Review> Reviews { get; }
        Task SaveChangesAsync();
    }
}