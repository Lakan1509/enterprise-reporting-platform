using EnterpriseReporting.Domain.Entities;
using EnterpriseReporting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.IntegrationTests;

public class UnitTest1
{
    private static ReportingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ReportingDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var context = new ReportingDbContext(options);

        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task Customer_CanBeSavedAndRetrieved()
    {
        await using var context = CreateContext();

        context.Customers.Add(new Customer
        {
            CustomerCode = "CUST-TEST",
            Name = "Test Customer",
            Region = "South",
            Email = "test@example.com"
        });

        await context.SaveChangesAsync();

        var customer =
            await context.Customers.SingleAsync();

        Assert.Equal("CUST-TEST", customer.CustomerCode);
        Assert.Equal("Test Customer", customer.Name);
        Assert.Equal("South", customer.Region);
    }

    [Fact]
    public async Task Product_CanBeSavedAndRetrieved()
    {
        await using var context = CreateContext();

        context.Products.Add(new Product
        {
            ProductCode = "PROD-TEST",
            Name = "Test Product",
            Category = "Software",
            UnitPrice = 499.99m
        });

        await context.SaveChangesAsync();

        var product =
            await context.Products.SingleAsync();

        Assert.Equal("PROD-TEST", product.ProductCode);
        Assert.Equal(499.99m, product.UnitPrice);
    }

    [Fact]
    public async Task Order_CanBeLinkedToCustomer()
    {
        await using var context = CreateContext();

        var customer = new Customer
        {
            CustomerCode = "CUST-ORDER",
            Name = "Order Customer",
            Region = "Midwest",
            Email = "order@example.com"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        context.Orders.Add(new Order
        {
            OrderNumber = "ORD-2000",
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 1200m,
            Status = "Completed"
        });

        await context.SaveChangesAsync();

        var order = await context.Orders
            .Include(x => x.Customer)
            .SingleAsync();

        Assert.NotNull(order.Customer);
        Assert.Equal("Order Customer", order.Customer!.Name);
        Assert.Equal(1200m, order.TotalAmount);
    }

    [Fact]
    public async Task SalesRecord_CanBePersisted()
    {
        await using var context = CreateContext();

        context.SalesRecords.Add(new SalesRecord
        {
            TransactionId = "TXN-TEST",
            CustomerCode = "CUST-001",
            ProductCode = "PROD-001",
            Region = "South",
            Amount = 2500m,
            TransactionDate = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var record =
            await context.SalesRecords.SingleAsync();

        Assert.Equal("TXN-TEST", record.TransactionId);
        Assert.Equal(2500m, record.Amount);
    }
}
