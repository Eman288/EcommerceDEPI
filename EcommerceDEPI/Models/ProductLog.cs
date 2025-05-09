namespace EcommerceDEPI.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class ProductLog
    {
        public int Id { get; set; }
        public DateTime ActionDate { get; set; }
        public string Content { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string UserId { get; set; }
        public User User { get; set; } // Admin who changed the product
    }
}