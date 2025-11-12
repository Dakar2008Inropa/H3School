using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GudumholmIF.Migrations
{
    /// <inheritdoc />
    public partial class PersonSport_SurrogateKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonSports",
                table: "PersonSports");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PersonSports",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonSports",
                table: "PersonSports",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PersonSports_PersonId_SportId",
                table: "PersonSports",
                columns: new[] { "PersonId", "SportId" },
                unique: true,
                filter: "[Left] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonSports",
                table: "PersonSports");

            migrationBuilder.DropIndex(
                name: "IX_PersonSports_PersonId_SportId",
                table: "PersonSports");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PersonSports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonSports",
                table: "PersonSports",
                columns: new[] { "PersonId", "SportId", "Joined" });
        }
    }
}
