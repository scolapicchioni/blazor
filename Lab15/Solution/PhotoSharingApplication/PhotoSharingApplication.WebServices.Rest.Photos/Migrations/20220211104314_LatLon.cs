using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoSharingApplication.WebServices.Rest.Photos.Migrations
{
    public partial class LatLon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Photos",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Photos",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Photos");
        }
    }
}
