using EnterpriseReporting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseReporting.Infrastructure.Data;

public class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<SalesRecord> SalesRecords => Set<SalesRecord>();
    public DbSet<IntegrationJob> IntegrationJobs => Set<IntegrationJob>();
    public DbSet<ValidationError> ValidationErrors => Set<ValidationError>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.CustomerCode).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Region).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(200);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.ProductCode).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(50);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalesRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransactionId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.TransactionId).IsUnique();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<IntegrationJob>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceSystem).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<ValidationError>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RecordIdentifier).HasMaxLength(100);
            entity.Property(x => x.FieldName).HasMaxLength(100);
            entity.Property(x => x.ErrorMessage).HasMaxLength(1000).IsRequired();
        });
    }
}
