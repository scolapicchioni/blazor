using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoSharingApplication.WebServices.REST.Photos.Migrations
{
    /// <inheritdoc />
    public partial class LatLon : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
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
