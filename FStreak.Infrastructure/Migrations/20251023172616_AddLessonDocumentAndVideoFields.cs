using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FStreak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonDocumentAndVideoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "Lessons",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "Lessons",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Lessons",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Lessons");
        }
    }
}
