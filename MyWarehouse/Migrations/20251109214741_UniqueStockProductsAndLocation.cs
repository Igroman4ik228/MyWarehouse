using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class UniqueStockProductsAndLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CURS_Stocks_ProductId",
                table: "CURS_Stocks");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Stocks_ProductId_LocationId",
                table: "CURS_Stocks",
                columns: new[] { "ProductId", "LocationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CURS_Stocks_ProductId_LocationId",
                table: "CURS_Stocks");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Stocks_ProductId",
                table: "CURS_Stocks",
                column: "ProductId");
        }
    }
}
