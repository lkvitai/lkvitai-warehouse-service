using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ValueAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "value_adjustments",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePhysicalId = table.Column<Guid>(type: "uuid", nullable: false),
                    BinId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeltaValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    user = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_value_adjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_value_adjustments_bin_BinId",
                        column: x => x.BinId,
                        principalSchema: "wh",
                        principalTable: "bin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_value_adjustments_item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "wh",
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_value_adjustments_warehouse_physical_WarehousePhysicalId",
                        column: x => x.WarehousePhysicalId,
                        principalSchema: "wh",
                        principalTable: "warehouse_physical",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustments_ItemId_WarehousePhysicalId_BinId_BatchId",
                schema: "wh",
                table: "value_adjustments",
                columns: new[] { "ItemId", "WarehousePhysicalId", "BinId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustments_Timestamp",
                schema: "wh",
                table: "value_adjustments",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "value_adjustments",
                schema: "wh");
        }
    }
}
