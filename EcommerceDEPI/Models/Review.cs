namespace EcommerceDEPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Review
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    [ForeignKey(nameof(OrderId))] // تحديد إن OrderId هو الـ Foreign Key
    public Order? Order { get; set; } // جعل Order nullable

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] // تحديد إن ProductId هو الـ Foreign Key
    public Product? Product { get; set; } // جعل Product nullable

    public string UserId { get; set; }
    [ForeignKey(nameof(UserId))] // تحديد إن UserId هو الـ Foreign Key
    public User? User { get; set; } // جعل User nullable

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Comment is required")]
    public string Comment { get; set; }

    public DateTime ReviewDate { get; set; }
}