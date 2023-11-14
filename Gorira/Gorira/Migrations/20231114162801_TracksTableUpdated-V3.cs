using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gorira.Migrations
{
    public partial class TracksTableUpdatedV3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasFree",
                table: "Tracks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasFree",
                table: "Tracks");
        }
    }
}
