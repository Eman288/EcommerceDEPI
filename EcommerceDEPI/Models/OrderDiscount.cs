namespace EcommerceDEPI.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class OrderDiscount
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int DiscountId { get; set; }
        public Discount Discount { get; set; }
    }

}