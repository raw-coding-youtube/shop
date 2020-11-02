using Microsoft.EntityFrameworkCore.Migrations;

namespace RawCoding.Shop.Database.Migrations
{
    public partial class published : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Published",
                table: "Products",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Published",
                table: "Products");
        }
    }
}
