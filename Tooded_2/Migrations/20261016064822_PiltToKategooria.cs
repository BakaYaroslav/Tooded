using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_2.Migrations
{
    /// <inheritdoc />
    public partial class PiltToKategooria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pilt",
                table: "Toodetabel");

            migrationBuilder.AddColumn<string>(
                name: "Pilt",
                table: "Kategooriatabel",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pilt",
                table: "Kategooriatabel");

            migrationBuilder.AddColumn<string>(
                name: "Pilt",
                table: "Toodetabel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
