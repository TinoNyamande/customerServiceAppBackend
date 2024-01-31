using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailCustomerServiceApi.Migrations
{
    /// <inheritdoc />
    public partial class emailResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComplainResponse",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplainResponse",
                table: "Emails");
        }
    }
}
