using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260302221000_SwitchEmailToUsernameAuth")]
public partial class SwitchEmailToUsernameAuth : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            table: "Users");

        migrationBuilder.RenameColumn(
            name: "Email",
            table: "Users",
            newName: "Username");

        migrationBuilder.AlterColumn<string>(
            name: "Username",
            table: "Users",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(320)",
            oldMaxLength: 320);

        migrationBuilder.Sql("""
            UPDATE "Users"
            SET "Username" = lower(trim("Username"))
            WHERE "Username" IS NOT NULL;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Username",
            table: "Users",
            column: "Username",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Username",
            table: "Users");

        migrationBuilder.AlterColumn<string>(
            name: "Username",
            table: "Users",
            type: "character varying(320)",
            maxLength: 320,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(32)",
            oldMaxLength: 32);

        migrationBuilder.RenameColumn(
            name: "Username",
            table: "Users",
            newName: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }
}
