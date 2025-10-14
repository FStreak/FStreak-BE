using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FStreak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_StudyRoom_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StudyRooms",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                table: "StudyRooms",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StudyRooms");

            migrationBuilder.DropColumn(
                name: "MeetingLink",
                table: "StudyRooms");
        }
    }
}
