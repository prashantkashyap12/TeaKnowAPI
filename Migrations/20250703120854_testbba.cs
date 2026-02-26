using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace userPanelOMR.Migrations
{
    /// <inheritdoc />
    public partial class testbba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "singUps",
                newName: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userId",
                table: "singUps",
                newName: "Id");
        }
    }
}
