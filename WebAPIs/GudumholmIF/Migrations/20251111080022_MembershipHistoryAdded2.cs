using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GudumholmIF.Migrations
{
    /// <inheritdoc />
    public partial class MembershipHistoryAdded2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "MembershipHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "MembershipHistories");
        }
    }
}
