using Microsoft.AspNetCore.Identity;
namespace EcommerceDEPI.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public string Picture { get; set; } = "/images/default-profile.png"; // صورة افتراضية

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }
}