using EnterpriseReporting.Domain.Entities;

namespace EnterpriseReporting.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Customer_DefaultValues_ShouldBeEmptyStrings()
    {
        var customer = new Customer();

        Assert.Equal(string.Empty, customer.CustomerCode);
        Assert.Equal(string.Empty, customer.Name);
        Assert.Equal(string.Empty, customer.Region);
        Assert.Equal(string.Empty, customer.Email);
        Assert.NotNull(customer.Orders);
    }

    [Fact]
    public void Product_UnitPrice_ShouldStoreDecimalValue()
    {
        var product = new Product
        {
            ProductCode = "PROD-TEST",
            Name = "Test Product",
            Category = "Analytics",
            UnitPrice = 999.99m
        };

        Assert.Equal(999.99m, product.UnitPrice);
        Assert.Equal("PROD-TEST", product.ProductCode);
    }

    [Fact]
    public void Order_ShouldStoreCustomerAndAmount()
    {
        var order = new Order
        {
            OrderNumber = "ORD-TEST",
            CustomerId = 10,
            TotalAmount = 1500m,
            Status = "Completed"
        };

        Assert.Equal("ORD-TEST", order.OrderNumber);
        Assert.Equal(10, order.CustomerId);
        Assert.Equal(1500m, order.TotalAmount);
        Assert.Equal("Completed", order.Status);
    }

    [Fact]
    public void IntegrationJob_DefaultStatus_ShouldBePending()
    {
        var job = new IntegrationJob();

        Assert.Equal("Pending", job.Status);
        Assert.Equal(0, job.TotalRecords);
        Assert.Equal(0, job.SuccessfulRecords);
        Assert.Equal(0, job.FailedRecords);
    }
}
