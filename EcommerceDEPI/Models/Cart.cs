using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceDEPI.Models
{
    public class Cart
    {
        public int Id { get; set; }      // معرف السلة
        public string UserId { get; set; }  // معرف المستخدم المرتبط بالسلة
        public User User { get; set; }    // العلاقة مع المستخدم
        public ICollection<CartProduct> CartProducts { get; set; } = new List<CartProduct>();  // قائمة المنتجات في السلة
    }
}
