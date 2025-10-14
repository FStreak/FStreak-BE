using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FStreak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_StudyRoom_Table_StartTime_EndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "StudyRooms",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "StudyRooms",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "StudyRooms");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "StudyRooms");
        }
    }
}
