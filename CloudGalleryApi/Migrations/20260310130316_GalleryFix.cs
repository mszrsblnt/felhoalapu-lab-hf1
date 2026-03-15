using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudGalleryApi.Migrations
{
    /// <inheritdoc />
    public partial class GalleryFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "Galleries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "Galleries",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
