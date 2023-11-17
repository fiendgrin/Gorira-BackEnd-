using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gorira.Migrations
{
    public partial class PlayTokensTableAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TrackId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "date", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "date", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayTokens_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayTokens_TrackId",
                table: "PlayTokens",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayTokens_UserId",
                table: "PlayTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayTokens");
        }
    }
}
