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
    public DbSet<UserMonthlyCashflow> UserMonthlyCashflows => Set<UserMonthlyCashflow>();
    public DbSet<UserMonthlyIncome> UserMonthlyIncomes => Set<UserMonthlyIncome>();
    public DbSet<UserMonthlyFixedExpense> UserMonthlyFixedExpenses => Set<UserMonthlyFixedExpense>();
    public DbSet<UserMonthlyVariableExpense> UserMonthlyVariableExpenses => Set<UserMonthlyVariableExpense>();
    public DbSet<UserMonthlyTransaction> UserMonthlyTransactions => Set<UserMonthlyTransaction>();

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

        modelBuilder.Entity<UserMonthlyCashflow>(builder =>
        {
            builder.ToTable("UserMonthlyCashflows");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.UserId, x.Year, x.Month }).IsUnique();
            builder.Property(x => x.StartBalance).HasPrecision(18, 2);
            builder.Property(x => x.SavingsStart).HasPrecision(18, 2);
            builder.Property(x => x.WithdrawnFromSavings).HasPrecision(18, 2);
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<UserMonthlyIncome>(builder =>
        {
            builder.ToTable("UserMonthlyIncomes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Date).HasColumnType("date");
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Label).HasMaxLength(200);
            builder.HasOne<UserMonthlyCashflow>()
                .WithMany(x => x.Incomes)
                .HasForeignKey(x => x.UserMonthlyCashflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMonthlyFixedExpense>(builder =>
        {
            builder.ToTable("UserMonthlyFixedExpenses");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.DueDate).HasColumnType("date");
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.Category).HasMaxLength(200);
            builder.HasOne<UserMonthlyCashflow>()
                .WithMany(x => x.FixedExpenses)
                .HasForeignKey(x => x.UserMonthlyCashflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMonthlyVariableExpense>(builder =>
        {
            builder.ToTable("UserMonthlyVariableExpenses");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Date).HasColumnType("date");
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Label).HasMaxLength(200);
            builder.HasOne<UserMonthlyCashflow>()
                .WithMany(x => x.VariableExpenses)
                .HasForeignKey(x => x.UserMonthlyCashflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMonthlyTransaction>(builder =>
        {
            builder.ToTable("UserMonthlyTransactions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Date).HasColumnType("date");
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Label).HasMaxLength(200);
            builder.HasOne<UserMonthlyCashflow>()
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.UserMonthlyCashflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
