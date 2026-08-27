using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseReporting.Infrastructure.Data;

public class AzureReportingDbContextFactory
    : IDesignTimeDbContextFactory<AzureReportingDbContext>
{
    public AzureReportingDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__ReportingDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__ReportingDatabase is not configured.");
        }

        var options = new DbContextOptionsBuilder<AzureReportingDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AzureReportingDbContext(options);
    }
}
