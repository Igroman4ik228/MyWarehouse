using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CURS_Categories",
                columns: table => new
                {
                    IdCategory = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Categories", x => x.IdCategory);
                });

            migrationBuilder.CreateTable(
                name: "CURS_Clients",
                columns: table => new
                {
                    IdClient = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Clients", x => x.IdClient);
                });

            migrationBuilder.CreateTable(
                name: "CURS_DeliveryTypes",
                columns: table => new
                {
                    IdDeliveryType = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_DeliveryTypes", x => x.IdDeliveryType);
                });

            migrationBuilder.CreateTable(
                name: "CURS_Locations",
                columns: table => new
                {
                    IdLocation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Shelf = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cell = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxLength = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxWidth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxHeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Locations", x => x.IdLocation);
                });

            migrationBuilder.CreateTable(
                name: "CURS_Roles",
                columns: table => new
                {
                    IdRole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Roles", x => x.IdRole);
                });

            migrationBuilder.CreateTable(
                name: "CURS_TaskStatuses",
                columns: table => new
                {
                    IdTaskStatus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_TaskStatuses", x => x.IdTaskStatus);
                });

            migrationBuilder.CreateTable(
                name: "CURS_Units",
                columns: table => new
                {
                    IdUnit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Units", x => x.IdUnit);
                });

            migrationBuilder.CreateTable(
                name: "CURS_Users",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    Patronymic = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Users", x => x.IdUser);
                    table.ForeignKey(
                        name: "FK_CURS_Users_CURS_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "CURS_Roles",
                        principalColumn: "IdRole",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CURS_Products",
                columns: table => new
                {
                    IdProduct = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ManufacturerId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFragile = table.Column<bool>(type: "bit", nullable: false),
                    IsWaterSensitive = table.Column<bool>(type: "bit", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Products", x => x.IdProduct);
                    table.ForeignKey(
                        name: "FK_CURS_Products_CURS_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CURS_Categories",
                        principalColumn: "IdCategory",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_Products_CURS_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "CURS_Units",
                        principalColumn: "IdUnit",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CURS_DeliveryTasks",
                columns: table => new
                {
                    IdDeliveryTask = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ProductQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedUserId = table.Column<int>(type: "int", nullable: false),
                    TaskStatusId = table.Column<int>(type: "int", nullable: false),
                    DeliveryTypeId = table.Column<int>(type: "int", nullable: false),
                    ExecutorUserId = table.Column<int>(type: "int", nullable: true),
                    FromLocationId = table.Column<int>(type: "int", nullable: true),
                    ToLocationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_DeliveryTasks", x => x.IdDeliveryTask);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "CURS_Clients",
                        principalColumn: "IdClient",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_DeliveryTypes_DeliveryTypeId",
                        column: x => x.DeliveryTypeId,
                        principalTable: "CURS_DeliveryTypes",
                        principalColumn: "IdDeliveryType",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_Locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "CURS_Locations",
                        principalColumn: "IdLocation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_Locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "CURS_Locations",
                        principalColumn: "IdLocation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "CURS_Products",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_TaskStatuses_TaskStatusId",
                        column: x => x.TaskStatusId,
                        principalTable: "CURS_TaskStatuses",
                        principalColumn: "IdTaskStatus",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_Users_CreatedUserId",
                        column: x => x.CreatedUserId,
                        principalTable: "CURS_Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_DeliveryTasks_CURS_Users_ExecutorUserId",
                        column: x => x.ExecutorUserId,
                        principalTable: "CURS_Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CURS_Stocks",
                columns: table => new
                {
                    IdStocks = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductQuantity = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    MaxProductQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURS_Stocks", x => x.IdStocks);
                    table.ForeignKey(
                        name: "FK_CURS_Stocks_CURS_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CURS_Locations",
                        principalColumn: "IdLocation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURS_Stocks_CURS_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "CURS_Products",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_ClientId",
                table: "CURS_DeliveryTasks",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_CreatedAt",
                table: "CURS_DeliveryTasks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_CreatedUserId",
                table: "CURS_DeliveryTasks",
                column: "CreatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_DeliveryTypeId",
                table: "CURS_DeliveryTasks",
                column: "DeliveryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_ExecutorUserId",
                table: "CURS_DeliveryTasks",
                column: "ExecutorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_FromLocationId",
                table: "CURS_DeliveryTasks",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_ProductId",
                table: "CURS_DeliveryTasks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_TaskStatusId",
                table: "CURS_DeliveryTasks",
                column: "TaskStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_DeliveryTasks_ToLocationId",
                table: "CURS_DeliveryTasks",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Products_CategoryId",
                table: "CURS_Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Products_SKU",
                table: "CURS_Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Products_UnitId",
                table: "CURS_Products",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Stocks_LocationId",
                table: "CURS_Stocks",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Stocks_ProductId",
                table: "CURS_Stocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Users_Login",
                table: "CURS_Users",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CURS_Users_RoleId",
                table: "CURS_Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CURS_DeliveryTasks");

            migrationBuilder.DropTable(
                name: "CURS_Stocks");

            migrationBuilder.DropTable(
                name: "CURS_Clients");

            migrationBuilder.DropTable(
                name: "CURS_DeliveryTypes");

            migrationBuilder.DropTable(
                name: "CURS_TaskStatuses");

            migrationBuilder.DropTable(
                name: "CURS_Users");

            migrationBuilder.DropTable(
                name: "CURS_Locations");

            migrationBuilder.DropTable(
                name: "CURS_Products");

            migrationBuilder.DropTable(
                name: "CURS_Roles");

            migrationBuilder.DropTable(
                name: "CURS_Categories");

            migrationBuilder.DropTable(
                name: "CURS_Units");
        }
    }
}
