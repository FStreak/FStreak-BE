using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FStreak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMetadataJsonToImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "ShopItems",
                newName: "Image");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Payments",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image",
                table: "ShopItems",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Payments",
                newName: "description");
        }
    }
}
