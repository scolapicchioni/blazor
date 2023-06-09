using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhotoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");
        }
    }
}
