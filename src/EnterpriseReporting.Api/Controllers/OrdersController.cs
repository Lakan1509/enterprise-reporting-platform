using EnterpriseReporting.Domain.Entities;
using EnterpriseReporting.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ReportingDbContext _context;

    public OrdersController(ReportingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .OrderByDescending(x => x.OrderDate)
            .Select(x => new
            {
                x.Id,
                x.OrderNumber,
                x.CustomerId,
                CustomerName = x.Customer != null
                    ? x.Customer.Name
                    : null,
                CustomerRegion = x.Customer != null
                    ? x.Customer.Region
                    : null,
                x.OrderDate,
                x.TotalAmount,
                x.Status
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.OrderNumber,
                x.CustomerId,
                CustomerName = x.Customer != null
                    ? x.Customer.Name
                    : null,
                CustomerRegion = x.Customer != null
                    ? x.Customer.Region
                    : null,
                x.OrderDate,
                x.TotalAmount,
                x.Status
            })
            .FirstOrDefaultAsync();

        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.OrderNumber))
            return BadRequest("OrderNumber is required.");

        if (order.TotalAmount <= 0)
            return BadRequest("TotalAmount must be greater than zero.");

        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == order.CustomerId);

        if (!customerExists)
            return BadRequest(
                $"Customer {order.CustomerId} does not exist.");

        var orderExists = await _context.Orders
            .AnyAsync(x => x.OrderNumber == order.OrderNumber);

        if (orderExists)
            return Conflict(
                $"Order number '{order.OrderNumber}' already exists.");

        if (order.OrderDate == default)
            order.OrderDate = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(order.Status))
            order.Status = "Pending";

        order.Customer = null;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            new
            {
                order.Id,
                order.OrderNumber,
                order.CustomerId,
                order.OrderDate,
                order.TotalAmount,
                order.Status
            });
    }
}
