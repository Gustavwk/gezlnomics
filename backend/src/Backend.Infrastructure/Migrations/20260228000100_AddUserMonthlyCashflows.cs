using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations;

public partial class AddUserMonthlyCashflows : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserMonthlyCashflows",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Year = table.Column<int>(type: "integer", nullable: false),
                Month = table.Column<int>(type: "integer", nullable: false),
                StartBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                SavingsStart = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                WithdrawnFromSavings = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserMonthlyCashflows", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserMonthlyFixedExpenses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserMonthlyCashflowId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                DueDayOfMonth = table.Column<int>(type: "integer", nullable: true),
                Frequency = table.Column<int>(type: "integer", nullable: false),
                Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserMonthlyFixedExpenses", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserMonthlyFixedExpenses_UserMonthlyCashflows_UserMonthlyCashflowId",
                    column: x => x.UserMonthlyCashflowId,
                    principalTable: "UserMonthlyCashflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserMonthlyIncomes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserMonthlyCashflowId = table.Column<Guid>(type: "uuid", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserMonthlyIncomes", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserMonthlyIncomes_UserMonthlyCashflows_UserMonthlyCashflowId",
                    column: x => x.UserMonthlyCashflowId,
                    principalTable: "UserMonthlyCashflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserMonthlyTransactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserMonthlyCashflowId = table.Column<Guid>(type: "uuid", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserMonthlyTransactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserMonthlyTransactions_UserMonthlyCashflows_UserMonthlyCashflowId",
                    column: x => x.UserMonthlyCashflowId,
                    principalTable: "UserMonthlyCashflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserMonthlyVariableExpenses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserMonthlyCashflowId = table.Column<Guid>(type: "uuid", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserMonthlyVariableExpenses", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserMonthlyVariableExpenses_UserMonthlyCashflows_UserMonthlyCashflowId",
                    column: x => x.UserMonthlyCashflowId,
                    principalTable: "UserMonthlyCashflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserMonthlyCashflows_UserId_Year_Month",
            table: "UserMonthlyCashflows",
            columns: new[] { "UserId", "Year", "Month" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserMonthlyFixedExpenses_UserMonthlyCashflowId",
            table: "UserMonthlyFixedExpenses",
            column: "UserMonthlyCashflowId");

        migrationBuilder.CreateIndex(
            name: "IX_UserMonthlyIncomes_UserMonthlyCashflowId",
            table: "UserMonthlyIncomes",
            column: "UserMonthlyCashflowId");

        migrationBuilder.CreateIndex(
            name: "IX_UserMonthlyTransactions_UserMonthlyCashflowId",
            table: "UserMonthlyTransactions",
            column: "UserMonthlyCashflowId");

        migrationBuilder.CreateIndex(
            name: "IX_UserMonthlyVariableExpenses_UserMonthlyCashflowId",
            table: "UserMonthlyVariableExpenses",
            column: "UserMonthlyCashflowId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "UserMonthlyFixedExpenses");
        migrationBuilder.DropTable(name: "UserMonthlyIncomes");
        migrationBuilder.DropTable(name: "UserMonthlyTransactions");
        migrationBuilder.DropTable(name: "UserMonthlyVariableExpenses");
        migrationBuilder.DropTable(name: "UserMonthlyCashflows");
    }
}
