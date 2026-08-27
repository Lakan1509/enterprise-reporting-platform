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
    public async Task<IActionResult> CreateOrder(
        EnterpriseReporting.Api.Contracts.CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OrderNumber))
            return BadRequest("OrderNumber is required.");

        if (request.TotalAmount <= 0)
            return BadRequest("TotalAmount must be greater than zero.");

        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == request.CustomerId);

        if (!customerExists)
            return BadRequest(
                $"Customer {request.CustomerId} does not exist.");

        var orderExists = await _context.Orders
            .AnyAsync(x => x.OrderNumber == request.OrderNumber);

        if (orderExists)
            return Conflict(
                $"Order number '{request.OrderNumber}' already exists.");

        var order = new Order
        {
            OrderNumber = request.OrderNumber,
            CustomerId = request.CustomerId,
            OrderDate = request.OrderDate ?? DateTime.UtcNow,
            TotalAmount = request.TotalAmount,
            Status = string.IsNullOrWhiteSpace(request.Status)
                ? "Pending"
                : request.Status
        };

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
