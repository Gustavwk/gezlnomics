using Backend.Domain;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<StartingCapital> StartingCapitals => Set<StartingCapital>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(builder =>
        {
            builder.ToTable("Expenses");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Amount).HasPrecision(18, 2);
            builder.Property(e => e.Date).HasColumnType("date");
            builder.Property(e => e.Category).HasMaxLength(200);
            builder.Property(e => e.Note).HasMaxLength(1000);
            builder.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<StartingCapital>(builder =>
        {
            builder.ToTable("StartingCapitals");
            builder.HasKey(sc => sc.Id);
            builder.Property(sc => sc.Amount).HasPrecision(18, 2);
            builder.Property(sc => sc.Date).HasColumnType("date");
            builder.Property(sc => sc.CreatedAt).HasColumnType("timestamp with time zone");
        });
    }
}
