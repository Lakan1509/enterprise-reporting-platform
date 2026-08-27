using EnterpriseReporting.Domain.Entities;
using EnterpriseReporting.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationController : ControllerBase
{
    private readonly ReportingDbContext _context;

    public IntegrationController(ReportingDbContext context)
    {
        _context = context;
    }

    [HttpPost("import-sales-csv")]
    public async Task<IActionResult> ImportSalesCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("CSV file is required.");

        var job = new IntegrationJob
        {
            SourceSystem = "CSV",
            Status = "Running",
            StartedAt = DateTime.UtcNow
        };

        _context.IntegrationJobs.Add(job);
        await _context.SaveChangesAsync();

        var successful = 0;
        var failed = 0;
        var total = 0;

        using var reader = new StreamReader(file.OpenReadStream());

        var header = await reader.ReadLineAsync();

        if (header == null)
            return BadRequest("CSV file is empty.");

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            total++;

            var columns = line.Split(',');

            if (columns.Length != 6)
            {
                failed++;

                _context.ValidationErrors.Add(new ValidationError
                {
                    IntegrationJobId = job.Id,
                    RecordIdentifier = $"Row-{total}",
                    FieldName = "CSV",
                    ErrorMessage = "Expected 6 columns."
                });

                continue;
            }

            var transactionId = columns[0].Trim();
            var customerCode = columns[1].Trim();
            var productCode = columns[2].Trim();
            var region = columns[3].Trim();

            if (!decimal.TryParse(columns[4], out var amount))
            {
                failed++;

                _context.ValidationErrors.Add(new ValidationError
                {
                    IntegrationJobId = job.Id,
                    RecordIdentifier = transactionId,
                    FieldName = "Amount",
                    ErrorMessage = "Invalid amount."
                });

                continue;
            }

            if (!DateTime.TryParse(columns[5], out var transactionDate))
            {
                failed++;

                _context.ValidationErrors.Add(new ValidationError
                {
                    IntegrationJobId = job.Id,
                    RecordIdentifier = transactionId,
                    FieldName = "TransactionDate",
                    ErrorMessage = "Invalid transaction date."
                });

                continue;
            }

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                failed++;

                _context.ValidationErrors.Add(new ValidationError
                {
                    IntegrationJobId = job.Id,
                    RecordIdentifier = $"Row-{total}",
                    FieldName = "TransactionId",
                    ErrorMessage = "TransactionId is required."
                });

                continue;
            }

            if (amount <= 0)
            {
                failed++;

                _context.ValidationErrors.Add(new ValidationError
                {
                    IntegrationJobId = job.Id,
                    RecordIdentifier = transactionId,
                    FieldName = "Amount",
                    ErrorMessage = "Amount must be greater than zero."
                });

                continue;
            }

            var duplicate = await _context.SalesRecords
                .AnyAsync(x => x.TransactionId == transactionId);

            if (duplicate)
            {
                failed++;

                _context.ValidationErrors.Add(new ValidationError
                {
                    IntegrationJobId = job.Id,
                    RecordIdentifier = transactionId,
                    FieldName = "TransactionId",
                    ErrorMessage = "Duplicate transaction."
                });

                continue;
            }

            _context.SalesRecords.Add(new SalesRecord
            {
                TransactionId = transactionId,
                CustomerCode = customerCode,
                ProductCode = productCode,
                Region = region,
                Amount = amount,
                TransactionDate = transactionDate
            });

            successful++;
        }

        job.TotalRecords = total;
        job.SuccessfulRecords = successful;
        job.FailedRecords = failed;
        job.Status = failed == 0 ? "Completed" : "CompletedWithErrors";
        job.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            jobId = job.Id,
            job.Status,
            totalRecords = total,
            successfulRecords = successful,
            failedRecords = failed
        });
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs()
    {
        var jobs = await _context.IntegrationJobs
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("errors")]
    public async Task<IActionResult> GetErrors()
    {
        var errors = await _context.ValidationErrors
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(errors);
    }
}
