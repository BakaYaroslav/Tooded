using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_2.Migrations
{
    /// <inheritdoc />
    public partial class LisaGarantiiKuudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GarantiiKuudes",
                table: "Toodetabel",
                type: "int",
                nullable: false,
                defaultValue: 24);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GarantiiKuudes",
                table: "Toodetabel");
        }
    }
}
