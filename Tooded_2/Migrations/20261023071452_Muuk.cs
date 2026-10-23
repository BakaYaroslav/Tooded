using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_2.Migrations
{
    /// <inheritdoc />
    public partial class Muuk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Muugitabel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Aeg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToodeId = table.Column<int>(type: "int", nullable: false),
                    Toodenimetus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kogus = table.Column<int>(type: "int", nullable: false),
                    Hind = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Summa = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muugitabel", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Muugitabel");
        }
    }
}
