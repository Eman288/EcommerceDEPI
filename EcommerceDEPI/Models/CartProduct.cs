using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceDEPI.Models
{
    public class CartProduct
    {
        public int CartId { get; set; }       // مفتاح أجنبي وجزء من المفتاح المركب
        public int ProductId { get; set; }    // مفتاح أجنبي وجزء من المفتاح المركب
        public int Quantity { get; set; }     // الكمية الخاصة بكل منتج في السلة

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}