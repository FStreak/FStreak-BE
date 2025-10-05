using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FStreak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyRoomFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudyRooms",
                columns: table => new
                {
                    StudyRoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    InviteCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyRooms", x => x.StudyRoomId);
                    table.ForeignKey(
                        name: "FK_StudyRooms_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomMessages",
                columns: table => new
                {
                    RoomMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudyRoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomMessages", x => x.RoomMessageId);
                    table.ForeignKey(
                        name: "FK_RoomMessages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomMessages_StudyRooms_StudyRoomId",
                        column: x => x.StudyRoomId,
                        principalTable: "StudyRooms",
                        principalColumn: "StudyRoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomUsers",
                columns: table => new
                {
                    RoomUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudyRoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    TotalStudyTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    StreakEarned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomUsers", x => x.RoomUserId);
                    table.ForeignKey(
                        name: "FK_RoomUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomUsers_StudyRooms_StudyRoomId",
                        column: x => x.StudyRoomId,
                        principalTable: "StudyRooms",
                        principalColumn: "StudyRoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomMessages_StudyRoomId",
                table: "RoomMessages",
                column: "StudyRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomMessages_UserId",
                table: "RoomMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomUsers_StudyRoomId",
                table: "RoomUsers",
                column: "StudyRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomUsers_UserId",
                table: "RoomUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyRooms_CreatedById",
                table: "StudyRooms",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomMessages");

            migrationBuilder.DropTable(
                name: "RoomUsers");

            migrationBuilder.DropTable(
                name: "StudyRooms");
        }
    }
}
