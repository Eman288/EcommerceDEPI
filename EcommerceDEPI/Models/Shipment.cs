using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceDEPI.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Shipment
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}