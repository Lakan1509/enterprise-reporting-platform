using EnterpriseReporting.Domain.Entities;
using EnterpriseReporting.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ReportingDbContext _context;

    public CustomersController(ReportingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(
        EnterpriseReporting.Api.Contracts.CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerCode))
            return BadRequest("CustomerCode is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var exists = await _context.Customers
            .AnyAsync(x => x.CustomerCode == request.CustomerCode);

        if (exists)
            return Conflict($"Customer code '{request.CustomerCode}' already exists.");

        var customer = new Customer
        {
            CustomerCode = request.CustomerCode,
            Name = request.Name,
            Region = request.Region,
            Email = request.Email
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id },
            customer);
    }
}
