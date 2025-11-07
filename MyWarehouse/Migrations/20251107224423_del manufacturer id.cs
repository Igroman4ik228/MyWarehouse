using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class delmanufacturerid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "CURS_Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManufacturerId",
                table: "CURS_Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
