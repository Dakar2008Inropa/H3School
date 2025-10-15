using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GUIWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModels1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageFileId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImageFiles",
                columns: table => new
                {
                    ImageFileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFiles", x => x.ImageFileId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ImageFileId",
                table: "Products",
                column: "ImageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageFiles_RelativePath",
                table: "ImageFiles",
                column: "RelativePath",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ImageFiles_ImageFileId",
                table: "Products",
                column: "ImageFileId",
                principalTable: "ImageFiles",
                principalColumn: "ImageFileId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ImageFiles_ImageFileId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ImageFiles");

            migrationBuilder.DropIndex(
                name: "IX_Products_ImageFileId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageFileId",
                table: "Products");
        }
    }
}
