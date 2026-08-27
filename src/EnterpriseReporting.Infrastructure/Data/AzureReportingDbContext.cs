using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.Infrastructure.Data;

public class AzureReportingDbContext : ReportingDbContext
{
    public AzureReportingDbContext(
        DbContextOptions<AzureReportingDbContext> options)
        : base(options)
    {
    }
}
