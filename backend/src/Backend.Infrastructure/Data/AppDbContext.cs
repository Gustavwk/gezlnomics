using Backend.Domain;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<IncomePeriod> IncomePeriods => Set<IncomePeriod>();
    public DbSet<LedgerTransaction> Transactions => Set<LedgerTransaction>();
    public DbSet<RecurringRule> RecurringRules => Set<RecurringRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Username).HasMaxLength(32);
            builder.HasIndex(x => x.Username).IsUnique();
            builder.Property(x => x.PasswordHash).HasMaxLength(500);
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<UserSettings>(builder =>
        {
            builder.ToTable("UserSettings");
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.CurrencyCode).HasMaxLength(8);
            builder.Property(x => x.Timezone).HasMaxLength(100);
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            builder.HasOne(x => x.User)
                .WithOne(x => x.Settings)
                .HasForeignKey<UserSettings>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IncomePeriod>(builder =>
        {
            builder.ToTable("IncomePeriods");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PeriodStartDate).HasColumnType("date");
            builder.Property(x => x.PeriodEndDate).HasColumnType("date");
            builder.Property(x => x.StartingBalance).HasPrecision(18, 2);
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            builder.HasIndex(x => new { x.UserId, x.PeriodStartDate, x.PeriodEndDate });
        });

        modelBuilder.Entity<LedgerTransaction>(builder =>
        {
            builder.ToTable("Transactions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Date).HasColumnType("date");
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Category).HasMaxLength(200);
            builder.Property(x => x.Note).HasMaxLength(1000);
            builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(40);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            builder.HasIndex(x => new { x.UserId, x.Date });
        });

        modelBuilder.Entity<RecurringRule>(builder =>
        {
            builder.ToTable("RecurringRules");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(200);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Category).HasMaxLength(200);
            builder.Property(x => x.Note).HasMaxLength(1000);
            builder.Property(x => x.RuleKind).HasConversion<string>().HasMaxLength(40);
            builder.Property(x => x.Frequency).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            builder.HasIndex(x => new { x.UserId, x.IsActive, x.StartDate });
        });
    }
}
