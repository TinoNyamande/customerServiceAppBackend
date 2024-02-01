using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailCustomerServiceApi.Migrations
{
    /// <inheritdoc />
    public partial class addFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromName",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeAssigned",
                table: "Emails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeResolved",
                table: "Emails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TimeTaken",
                table: "Emails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromName",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "TimeAssigned",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "TimeResolved",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "TimeTaken",
                table: "Emails");
        }
    }
}
