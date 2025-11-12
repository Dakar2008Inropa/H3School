using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GudumholmIF.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPersonSport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "PersonSports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "PersonSports");
        }
    }
}
