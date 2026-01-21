using System;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Backend.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20240915000000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.7")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("Backend.Domain.Expense", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<decimal>("Amount")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            b.Property<string>("Category")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateOnly>("Date")
                .HasColumnType("date");

            b.Property<string>("Note")
                .HasMaxLength(1000)
                .HasColumnType("character varying(1000)");

            b.HasKey("Id");

            b.ToTable("Expenses");
        });

        modelBuilder.Entity("Backend.Domain.StartingCapital", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<decimal>("Amount")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateOnly>("Date")
                .HasColumnType("date");

            b.HasKey("Id");

            b.ToTable("StartingCapitals");
        });
    }
}
