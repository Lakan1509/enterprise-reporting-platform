using EnterpriseReporting.Domain.Entities;
using EnterpriseReporting.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ReportingDbContext _context;

    public ProductsController(ReportingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return Ok(await _context.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.ProductCode))
            return BadRequest("ProductCode is required.");

        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Name is required.");

        if (product.UnitPrice < 0)
            return BadRequest("UnitPrice cannot be negative.");

        var exists = await _context.Products
            .AnyAsync(x => x.ProductCode == product.ProductCode);

        if (exists)
            return Conflict($"Product code '{product.ProductCode}' already exists.");

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product);
    }
}
