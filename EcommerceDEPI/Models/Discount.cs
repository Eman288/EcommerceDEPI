using System.ComponentModel.DataAnnotations;

namespace EcommerceDEPI.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Discount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        public decimal Amount { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();
        public bool IsActive { get; internal set; }
        public int Percentage { get; internal set; }
    }
}
