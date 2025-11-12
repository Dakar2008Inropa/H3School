using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GudumholmIF.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSportAndCreateSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnnualFee",
                table: "Sports",
                newName: "AnnualFeeChild");

            migrationBuilder.RenameColumn(
                name: "AnnualFee",
                table: "SportFeeHistories",
                newName: "AnnualFeeChild");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualFeeAdult",
                table: "Sports",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualFeeAdult",
                table: "SportFeeHistories",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PassiveAdultAnnualFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PassiveChildAnnualFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "PassiveAdultAnnualFee", "PassiveChildAnnualFee" },
                values: new object[] { 1, 400m, 200m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropColumn(
                name: "AnnualFeeAdult",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "AnnualFeeAdult",
                table: "SportFeeHistories");

            migrationBuilder.RenameColumn(
                name: "AnnualFeeChild",
                table: "Sports",
                newName: "AnnualFee");

            migrationBuilder.RenameColumn(
                name: "AnnualFeeChild",
                table: "SportFeeHistories",
                newName: "AnnualFee");
        }
    }
}
