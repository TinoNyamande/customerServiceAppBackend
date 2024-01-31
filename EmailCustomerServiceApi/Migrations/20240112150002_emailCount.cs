using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailCustomerServiceApi.Migrations
{
    /// <inheritdoc />
    public partial class emailCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveApplicationForm",
                table: "LeaveApplicationForm");

            migrationBuilder.RenameTable(
                name: "LeaveApplicationForm",
                newName: "Emails");

            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastAllocated",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emails",
                table: "Emails",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Emails",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "Available",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastAllocated",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Emails");

            migrationBuilder.RenameTable(
                name: "Emails",
                newName: "LeaveApplicationForm");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveApplicationForm",
                table: "LeaveApplicationForm",
                column: "Id");
        }
    }
}
