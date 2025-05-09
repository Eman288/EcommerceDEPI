namespace EcommerceDEPI.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class WishlistProduct
    {
        public int WishlistId { get; set; }
        public Wishlist Wishlist { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}