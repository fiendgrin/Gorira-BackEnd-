using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gorira.Migrations
{
    public partial class TracksTableUpdatedV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainGenreId",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrimaryMoodId",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondaryMoodId",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubGenreId",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_MainGenreId",
                table: "Tracks",
                column: "MainGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_PrimaryMoodId",
                table: "Tracks",
                column: "PrimaryMoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_SecondaryMoodId",
                table: "Tracks",
                column: "SecondaryMoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_SubGenreId",
                table: "Tracks",
                column: "SubGenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Genres_MainGenreId",
                table: "Tracks",
                column: "MainGenreId",
                principalTable: "Genres",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Genres_SubGenreId",
                table: "Tracks",
                column: "SubGenreId",
                principalTable: "Genres",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Moods_PrimaryMoodId",
                table: "Tracks",
                column: "PrimaryMoodId",
                principalTable: "Moods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Moods_SecondaryMoodId",
                table: "Tracks",
                column: "SecondaryMoodId",
                principalTable: "Moods",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Genres_MainGenreId",
                table: "Tracks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Genres_SubGenreId",
                table: "Tracks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Moods_PrimaryMoodId",
                table: "Tracks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Moods_SecondaryMoodId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_MainGenreId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_PrimaryMoodId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_SecondaryMoodId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_SubGenreId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "MainGenreId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "PrimaryMoodId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "SecondaryMoodId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "SubGenreId",
                table: "Tracks");
        }
    }
}
