using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gorira.Migrations
{
    public partial class PurchasesTableUpdatedV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Purchases",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Purchases");
        }
    }
}
