using Microsoft.EntityFrameworkCore.Migrations;

namespace Tooded_2.Migrations;

public partial class Shoping67Settings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
        name: "ShopSettings",
        columns: table => new
        {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_ShopSettings", x => x.Id));

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("ShopSettings");
}
