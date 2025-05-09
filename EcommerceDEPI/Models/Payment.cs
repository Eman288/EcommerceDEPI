using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceDEPI.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public string Method { get; set; }
        public decimal Amount { get; set; }
        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public string CardLastFour { get; set; } // Optional
        public string CardType { get; set; } // Optional
    }
}