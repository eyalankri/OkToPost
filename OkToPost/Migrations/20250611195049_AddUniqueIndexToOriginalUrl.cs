using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkToPost.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToOriginalUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OriginalUrl",
                table: "UrlMappings",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappings_OriginalUrl",
                table: "UrlMappings",
                column: "OriginalUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UrlMappings_OriginalUrl",
                table: "UrlMappings");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalUrl",
                table: "UrlMappings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);
        }
    }
}
