using EcommerceDEPI.Models;
using Microsoft.AspNetCore.Mvc;
using EcommerceDEPI.Data;

public class ShipmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShipmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // إضافة شحنة للأوردر
    [HttpPost]
    public IActionResult CreateShipment(int orderId, string address, string zipcode)
    {
        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return NotFound();

        var shipment = new Shipment
        {
            OrderId = order.Id,
            Address = address,
            Zipcode = zipcode
        };

        _context.Shipments.Add(shipment);
        _context.SaveChanges();

        order.Shipment = shipment;
        _context.SaveChanges();

        return RedirectToAction("Details", "Order", new { id = order.Id });
    }
}
