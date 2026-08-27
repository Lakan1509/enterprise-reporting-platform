using EnterpriseReporting.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ReportingDbContext _context;

    public DashboardController(ReportingDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var totalCustomers = await _context.Customers.CountAsync();
        var totalProducts = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();

        var totalRevenue = await _context.Orders
            .Where(x => x.Status == "Completed")
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0m;

        var averageOrderValue = totalOrders > 0
            ? await _context.Orders.AverageAsync(x => x.TotalAmount)
            : 0m;

        var completedOrders = await _context.Orders
            .CountAsync(x => x.Status == "Completed");

        var pendingOrders = await _context.Orders
            .CountAsync(x => x.Status == "Pending");

        return Ok(new
        {
            totalRevenue,
            totalCustomers,
            totalProducts,
            totalOrders,
            completedOrders,
            pendingOrders,
            averageOrderValue
        });
    }

    [HttpGet("sales-by-region")]
    public async Task<IActionResult> GetSalesByRegion()
    {
        var sales = await _context.Orders
            .AsNoTracking()
            .Where(x => x.Status == "Completed")
            .GroupBy(x => x.Customer!.Region)
            .Select(group => new
            {
                region = group.Key,
                orderCount = group.Count(),
                revenue = group.Sum(x => x.TotalAmount)
            })
            .OrderByDescending(x => x.revenue)
            .ToListAsync();

        return Ok(sales);
    }

    [HttpGet("recent-orders")]
    public async Task<IActionResult> GetRecentOrders()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.OrderDate)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                x.OrderNumber,
                customerName = x.Customer!.Name,
                region = x.Customer.Region,
                x.OrderDate,
                x.TotalAmount,
                x.Status
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("imported-sales-summary")]
    public async Task<IActionResult> GetImportedSalesSummary()
    {
        var totalImportedSales = await _context.SalesRecords
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        var totalImportedTransactions =
            await _context.SalesRecords.CountAsync();

        var salesByRegion = await _context.SalesRecords
            .AsNoTracking()
            .GroupBy(x => x.Region)
            .Select(group => new
            {
                region = group.Key,
                transactionCount = group.Count(),
                revenue = group.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.revenue)
            .ToListAsync();

        return Ok(new
        {
            totalImportedSales,
            totalImportedTransactions,
            salesByRegion
        });
    }

}
