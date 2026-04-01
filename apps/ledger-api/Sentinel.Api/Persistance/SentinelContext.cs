using Microsoft.EntityFrameworkCore;
using Sentinel.Api.Domain;

namespace Sentinel.Api.Persistance;

public class SentinelContext : DbContext
{
    public SentinelContext(DbContextOptions<SentinelContext> options) : base(options) { }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<CostCategory> CostCategories { get; set; }
    public DbSet<Cost> Costs { get; set; }
    public DbSet<AuditReport> AuditReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Senior Tip: Ensure Decimal precision for FinTech
        modelBuilder.Entity<Cost>()
            .Property(c => c.Amount)
            .HasConversion<double>(); // SQLite doesn't have a native decimal, double is safer for a 4-day MVP

        base.OnModelCreating(modelBuilder);
    }
}
