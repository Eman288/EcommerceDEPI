using EcommerceDEPI.Data.Interfaces;
using EcommerceDEPI.Data.Repositories;
using EcommerceDEPI.Models;

namespace EcommerceDEPI.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IRepository<Order> Orders { get; private set; }
        public IRepository<Cart> Carts { get; private set; }
        public IRepository<Discount> Discounts { get; private set; }
        public IRepository<CartProduct> CartProducts { get; private set; }
        public IRepository<Product> Products { get; private set; }
        public IRepository<Review> Reviews { get; private set; }

        public IRepository<OrderItem> OrderItems => throw new NotImplementedException();

        public IRepository<Payment> Payments => throw new NotImplementedException();

        public IRepository<Shipment> Shipments => throw new NotImplementedException();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Orders = new Repository<Order>(_context);
            Carts = new Repository<Cart>(_context);
            Discounts = new Repository<Discount>(_context);
            CartProducts = new Repository<CartProduct>(_context);
            Products = new Repository<Product>(_context);
            Reviews = new Repository<Review>(_context);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}