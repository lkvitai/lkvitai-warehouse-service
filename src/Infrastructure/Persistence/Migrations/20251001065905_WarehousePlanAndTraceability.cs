using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WarehousePlanAndTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Ref",
                schema: "wh",
                table: "movement",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BatchExpDate",
                schema: "wh",
                table: "movement",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BatchMfgDate",
                schema: "wh",
                table: "movement",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNo",
                schema: "wh",
                table: "movement",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchQuality",
                schema: "wh",
                table: "movement",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseRackId",
                schema: "wh",
                table: "bin",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseZoneId",
                schema: "wh",
                table: "bin",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "stock_batch",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNo = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MfgDate = table.Column<DateTime>(type: "date", nullable: true),
                    ExpDate = table.Column<DateTime>(type: "date", nullable: true),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    MetaJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_batch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_zone",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePhysicalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MetaJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse_zone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_warehouse_zone_warehouse_physical_WarehousePhysicalId",
                        column: x => x.WarehousePhysicalId,
                        principalSchema: "wh",
                        principalTable: "warehouse_physical",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "batch_lineage",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChildBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    QtyBase = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    MovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batch_lineage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_batch_lineage_movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "wh",
                        principalTable: "movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_batch_lineage_stock_batch_ChildBatchId",
                        column: x => x.ChildBatchId,
                        principalSchema: "wh",
                        principalTable: "stock_batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_batch_lineage_stock_batch_ParentBatchId",
                        column: x => x.ParentBatchId,
                        principalSchema: "wh",
                        principalTable: "stock_batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_rack",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MetaJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse_rack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_warehouse_rack_warehouse_zone_WarehouseZoneId",
                        column: x => x.WarehouseZoneId,
                        principalSchema: "wh",
                        principalTable: "warehouse_zone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustments_BinId",
                schema: "wh",
                table: "value_adjustments",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustments_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustments",
                column: "WarehousePhysicalId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_balance_BatchId",
                schema: "wh",
                table: "stock_balance",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_bin_WarehouseRackId",
                schema: "wh",
                table: "bin",
                column: "WarehouseRackId");

            migrationBuilder.CreateIndex(
                name: "IX_bin_WarehouseZoneId",
                schema: "wh",
                table: "bin",
                column: "WarehouseZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_batch_lineage_ChildBatchId",
                schema: "wh",
                table: "batch_lineage",
                column: "ChildBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_batch_lineage_MovementId",
                schema: "wh",
                table: "batch_lineage",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_batch_lineage_ParentBatchId",
                schema: "wh",
                table: "batch_lineage",
                column: "ParentBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_batch_ItemId_BatchNo",
                schema: "wh",
                table: "stock_batch",
                columns: new[] { "ItemId", "BatchNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_rack_WarehouseZoneId_Code",
                schema: "wh",
                table: "warehouse_rack",
                columns: new[] { "WarehouseZoneId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_zone_WarehousePhysicalId_Code",
                schema: "wh",
                table: "warehouse_zone",
                columns: new[] { "WarehousePhysicalId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_bin_warehouse_rack_WarehouseRackId",
                schema: "wh",
                table: "bin",
                column: "WarehouseRackId",
                principalSchema: "wh",
                principalTable: "warehouse_rack",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_bin_warehouse_zone_WarehouseZoneId",
                schema: "wh",
                table: "bin",
                column: "WarehouseZoneId",
                principalSchema: "wh",
                principalTable: "warehouse_zone",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_balance_stock_batch_BatchId",
                schema: "wh",
                table: "stock_balance",
                column: "BatchId",
                principalSchema: "wh",
                principalTable: "stock_batch",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bin_warehouse_rack_WarehouseRackId",
                schema: "wh",
                table: "bin");

            migrationBuilder.DropForeignKey(
                name: "FK_bin_warehouse_zone_WarehouseZoneId",
                schema: "wh",
                table: "bin");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_balance_stock_batch_BatchId",
                schema: "wh",
                table: "stock_balance");

            migrationBuilder.DropTable(
                name: "batch_lineage",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "warehouse_rack",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "stock_batch",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "warehouse_zone",
                schema: "wh");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustments_BinId",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustments_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropIndex(
                name: "IX_stock_balance_BatchId",
                schema: "wh",
                table: "stock_balance");

            migrationBuilder.DropIndex(
                name: "IX_bin_WarehouseRackId",
                schema: "wh",
                table: "bin");

            migrationBuilder.DropIndex(
                name: "IX_bin_WarehouseZoneId",
                schema: "wh",
                table: "bin");

            migrationBuilder.DropColumn(
                name: "BatchExpDate",
                schema: "wh",
                table: "movement");

            migrationBuilder.DropColumn(
                name: "BatchMfgDate",
                schema: "wh",
                table: "movement");

            migrationBuilder.DropColumn(
                name: "BatchNo",
                schema: "wh",
                table: "movement");

            migrationBuilder.DropColumn(
                name: "BatchQuality",
                schema: "wh",
                table: "movement");

            migrationBuilder.DropColumn(
                name: "WarehouseRackId",
                schema: "wh",
                table: "bin");

            migrationBuilder.DropColumn(
                name: "WarehouseZoneId",
                schema: "wh",
                table: "bin");

            migrationBuilder.AlterColumn<string>(
                name: "Ref",
                schema: "wh",
                table: "movement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
