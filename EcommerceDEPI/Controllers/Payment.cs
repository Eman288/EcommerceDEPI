using EcommerceDEPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EcommerceDEPI.Data;

public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;

    public PaymentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Pay(int orderId)
    {
        var order = _context.Orders.Include(o => o.OrderItems)
                                   .FirstOrDefault(o => o.Id == orderId);
        if (order == null) return NotFound();

        ViewBag.OrderTotal = order.TotalPrice;

        return View(order);
    }

    [HttpPost]
    public IActionResult ProcessPayment(int orderId, string paymentMethod)
    {
        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return NotFound();

        var payment = new Payment
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            OrderId = order.Id,
            Method = paymentMethod,
            Amount = order.TotalPrice,
            Date = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        _context.SaveChanges();

        order.Payment = payment;
        _context.SaveChanges();

        return RedirectToAction("Index", "Order");
    }
}
