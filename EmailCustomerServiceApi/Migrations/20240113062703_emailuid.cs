using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailCustomerServiceApi.Migrations
{
    /// <inheritdoc />
    public partial class emailuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailUid",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailUid",
                table: "Emails");
        }
    }
}
