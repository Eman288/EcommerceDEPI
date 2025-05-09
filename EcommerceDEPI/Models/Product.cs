using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceDEPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Picture { get; set; }
        public double Rate { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int Amount { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<WishlistProduct> WishlistProducts { get; set; } = new List<WishlistProduct>();
        public ICollection<CartProduct> CartProducts { get; set; } = new List<CartProduct>();
    }
}