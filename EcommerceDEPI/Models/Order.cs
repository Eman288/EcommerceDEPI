using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceDEPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        public string OrderStatus { get; set; } = "Pending"; 

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        public ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();
        public Shipment Shipment { get; set; }
        public Payment Payment { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>(); // العلاقة مع التقييمات
    }
}