using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudGalleryApi.Migrations
{
    /// <inheritdoc />
    public partial class PhotoBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Photos",
                newName: "ContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Photos",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Photos");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Photos",
                newName: "Url");
        }
    }
}
