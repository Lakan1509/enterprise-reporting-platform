using System.Text;
using EnterpriseReporting.Api.Controllers;
using EnterpriseReporting.Domain.Entities;
using EnterpriseReporting.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.IntegrationTests;

public class IntegrationWorkflowTests
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

    private static FormFile CreateCsvFile(string csv)
    {
        var bytes = Encoding.UTF8.GetBytes(csv);
        var stream = new MemoryStream(bytes);

        return new FormFile(
            stream,
            0,
            bytes.Length,
            "file",
            "sales.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }

    [Fact]
    public async Task ImportSalesCsv_ValidRows_ShouldPersistSalesRecords()
    {
        await using var context = CreateContext();
        var controller = new IntegrationController(context);

        var csv =
            "TransactionId,CustomerCode,ProductCode,Region,Amount,TransactionDate\n" +
            "TXN-1001,CUST-001,PROD-001,South,250.50,2026-08-01\n" +
            "TXN-1002,CUST-002,PROD-002,West,500.00,2026-08-02\n";

        var result = await controller.ImportSalesCsv(CreateCsvFile(csv));

        Assert.IsType<OkObjectResult>(result);

        Assert.Equal(2, await context.SalesRecords.CountAsync());
        Assert.Equal(0, await context.ValidationErrors.CountAsync());

        var job = await context.IntegrationJobs.SingleAsync();

        Assert.Equal(2, job.TotalRecords);
        Assert.Equal(2, job.SuccessfulRecords);
        Assert.Equal(0, job.FailedRecords);
        Assert.Equal("Completed", job.Status);
        Assert.NotNull(job.CompletedAt);
    }

    [Fact]
    public async Task ImportSalesCsv_InvalidAmount_ShouldRecordValidationError()
    {
        await using var context = CreateContext();
        var controller = new IntegrationController(context);

        var csv =
            "TransactionId,CustomerCode,ProductCode,Region,Amount,TransactionDate\n" +
            "TXN-2001,CUST-001,PROD-001,South,not-a-number,2026-08-01\n";

        var result = await controller.ImportSalesCsv(CreateCsvFile(csv));

        Assert.IsType<OkObjectResult>(result);

        Assert.Empty(await context.SalesRecords.ToListAsync());

        var error = await context.ValidationErrors.SingleAsync();

        Assert.Equal("TXN-2001", error.RecordIdentifier);
        Assert.Equal("Amount", error.FieldName);
        Assert.Equal("Invalid amount.", error.ErrorMessage);

        var job = await context.IntegrationJobs.SingleAsync();

        Assert.Equal(1, job.TotalRecords);
        Assert.Equal(0, job.SuccessfulRecords);
        Assert.Equal(1, job.FailedRecords);
        Assert.Equal("CompletedWithErrors", job.Status);
    }

    [Fact]
    public async Task ImportSalesCsv_InvalidDate_ShouldRecordValidationError()
    {
        await using var context = CreateContext();
        var controller = new IntegrationController(context);

        var csv =
            "TransactionId,CustomerCode,ProductCode,Region,Amount,TransactionDate\n" +
            "TXN-3001,CUST-001,PROD-001,South,250.00,not-a-date\n";

        await controller.ImportSalesCsv(CreateCsvFile(csv));

        var error = await context.ValidationErrors.SingleAsync();

        Assert.Equal("TXN-3001", error.RecordIdentifier);
        Assert.Equal("TransactionDate", error.FieldName);
        Assert.Equal("Invalid transaction date.", error.ErrorMessage);

        Assert.Empty(await context.SalesRecords.ToListAsync());
    }

    [Fact]
    public async Task ImportSalesCsv_NonPositiveAmount_ShouldRejectRecord()
    {
        await using var context = CreateContext();
        var controller = new IntegrationController(context);

        var csv =
            "TransactionId,CustomerCode,ProductCode,Region,Amount,TransactionDate\n" +
            "TXN-4001,CUST-001,PROD-001,South,0,2026-08-01\n";

        await controller.ImportSalesCsv(CreateCsvFile(csv));

        var error = await context.ValidationErrors.SingleAsync();

        Assert.Equal("Amount", error.FieldName);
        Assert.Equal(
            "Amount must be greater than zero.",
            error.ErrorMessage);

        Assert.Empty(await context.SalesRecords.ToListAsync());
    }

    [Fact]
    public async Task ImportSalesCsv_DuplicateTransaction_ShouldRejectDuplicate()
    {
        await using var context = CreateContext();

        context.SalesRecords.Add(new SalesRecord
        {
            TransactionId = "TXN-5001",
            CustomerCode = "CUST-001",
            ProductCode = "PROD-001",
            Region = "South",
            Amount = 100m,
            TransactionDate = new DateTime(2026, 8, 1)
        });

        await context.SaveChangesAsync();

        var controller = new IntegrationController(context);

        var csv =
            "TransactionId,CustomerCode,ProductCode,Region,Amount,TransactionDate\n" +
            "TXN-5001,CUST-002,PROD-002,West,700.00,2026-08-02\n";

        await controller.ImportSalesCsv(CreateCsvFile(csv));

        Assert.Equal(1, await context.SalesRecords.CountAsync());

        var error = await context.ValidationErrors.SingleAsync();

        Assert.Equal("TXN-5001", error.RecordIdentifier);
        Assert.Equal("TransactionId", error.FieldName);
        Assert.Equal("Duplicate transaction.", error.ErrorMessage);

        var job = await context.IntegrationJobs.SingleAsync();

        Assert.Equal(1, job.TotalRecords);
        Assert.Equal(0, job.SuccessfulRecords);
        Assert.Equal(1, job.FailedRecords);
        Assert.Equal("CompletedWithErrors", job.Status);
    }

    [Fact]
    public async Task ImportSalesCsv_MixedRows_ShouldTrackSuccessAndFailureCounts()
    {
        await using var context = CreateContext();
        var controller = new IntegrationController(context);

        var csv =
            "TransactionId,CustomerCode,ProductCode,Region,Amount,TransactionDate\n" +
            "TXN-6001,CUST-001,PROD-001,South,100.00,2026-08-01\n" +
            "TXN-6002,CUST-002,PROD-002,West,-20.00,2026-08-02\n" +
            "TXN-6003,CUST-003,PROD-003,East,300.00,2026-08-03\n";

        await controller.ImportSalesCsv(CreateCsvFile(csv));

        Assert.Equal(2, await context.SalesRecords.CountAsync());
        Assert.Equal(1, await context.ValidationErrors.CountAsync());

        var job = await context.IntegrationJobs.SingleAsync();

        Assert.Equal(3, job.TotalRecords);
        Assert.Equal(2, job.SuccessfulRecords);
        Assert.Equal(1, job.FailedRecords);
        Assert.Equal("CompletedWithErrors", job.Status);
    }
}
