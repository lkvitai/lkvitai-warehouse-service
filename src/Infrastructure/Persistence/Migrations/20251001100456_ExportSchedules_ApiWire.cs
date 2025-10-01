using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExportSchedules_ApiWire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustments_bin_BinId",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustments_item_ItemId",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustments_warehouse_physical_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropIndex(
                name: "IX_stock_balance_ItemId_WarehousePhysicalId_BinId_BatchId",
                schema: "wh",
                table: "stock_balance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_value_adjustments",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustments_ItemId_WarehousePhysicalId_BinId_BatchId",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustments_Timestamp",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.DropColumn(
                name: "user",
                schema: "wh",
                table: "value_adjustments");

            migrationBuilder.RenameTable(
                name: "value_adjustments",
                schema: "wh",
                newName: "value_adjustment",
                newSchema: "wh");

            migrationBuilder.RenameIndex(
                name: "IX_value_adjustments_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustment",
                newName: "IX_value_adjustment_WarehousePhysicalId");

            migrationBuilder.RenameIndex(
                name: "IX_value_adjustments_BinId",
                schema: "wh",
                table: "value_adjustment",
                newName: "IX_value_adjustment_BinId");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseLogicalId",
                schema: "wh",
                table: "stock_balance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "job",
                table: "export_schedule",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamptz",
                oldDefaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "AtUtc",
                schema: "job",
                table: "export_schedule",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "job",
                table: "export_job",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamptz",
                oldDefaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AlterColumn<Guid>(
                name: "WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustment",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeltaValue",
                schema: "wh",
                table: "value_adjustment",
                type: "numeric(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseLogicalId",
                schema: "wh",
                table: "value_adjustment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                schema: "wh",
                table: "value_adjustment",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_value_adjustment",
                schema: "wh",
                table: "value_adjustment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_balance_ItemId_WarehousePhysicalId_WarehouseLogicalId~",
                schema: "wh",
                table: "stock_balance",
                columns: new[] { "ItemId", "WarehousePhysicalId", "WarehouseLogicalId", "BinId", "BatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_balance_WarehouseLogicalId",
                schema: "wh",
                table: "stock_balance",
                column: "WarehouseLogicalId");

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustment_BatchId",
                schema: "wh",
                table: "value_adjustment",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustment_ItemId",
                schema: "wh",
                table: "value_adjustment",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustment_ItemId_WarehousePhysicalId_WarehouseLogica~",
                schema: "wh",
                table: "value_adjustment",
                columns: new[] { "ItemId", "WarehousePhysicalId", "WarehouseLogicalId", "BinId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustment_WarehouseLogicalId",
                schema: "wh",
                table: "value_adjustment",
                column: "WarehouseLogicalId");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_balance_warehouse_logical_WarehouseLogicalId",
                schema: "wh",
                table: "stock_balance",
                column: "WarehouseLogicalId",
                principalSchema: "wh",
                principalTable: "warehouse_logical",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustment_bin_BinId",
                schema: "wh",
                table: "value_adjustment",
                column: "BinId",
                principalSchema: "wh",
                principalTable: "bin",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustment_item_ItemId",
                schema: "wh",
                table: "value_adjustment",
                column: "ItemId",
                principalSchema: "wh",
                principalTable: "item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustment_stock_batch_BatchId",
                schema: "wh",
                table: "value_adjustment",
                column: "BatchId",
                principalSchema: "wh",
                principalTable: "stock_batch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustment_warehouse_logical_WarehouseLogicalId",
                schema: "wh",
                table: "value_adjustment",
                column: "WarehouseLogicalId",
                principalSchema: "wh",
                principalTable: "warehouse_logical",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustment_warehouse_physical_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustment",
                column: "WarehousePhysicalId",
                principalSchema: "wh",
                principalTable: "warehouse_physical",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stock_balance_warehouse_logical_WarehouseLogicalId",
                schema: "wh",
                table: "stock_balance");

            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustment_bin_BinId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustment_item_ItemId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustment_stock_batch_BatchId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustment_warehouse_logical_WarehouseLogicalId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_value_adjustment_warehouse_physical_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropIndex(
                name: "IX_stock_balance_ItemId_WarehousePhysicalId_WarehouseLogicalId~",
                schema: "wh",
                table: "stock_balance");

            migrationBuilder.DropIndex(
                name: "IX_stock_balance_WarehouseLogicalId",
                schema: "wh",
                table: "stock_balance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_value_adjustment",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustment_BatchId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustment_ItemId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustment_ItemId_WarehousePhysicalId_WarehouseLogica~",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropIndex(
                name: "IX_value_adjustment_WarehouseLogicalId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropColumn(
                name: "WarehouseLogicalId",
                schema: "wh",
                table: "stock_balance");

            migrationBuilder.DropColumn(
                name: "AtUtc",
                schema: "job",
                table: "export_schedule");

            migrationBuilder.DropColumn(
                name: "WarehouseLogicalId",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "wh",
                table: "value_adjustment");

            migrationBuilder.RenameTable(
                name: "value_adjustment",
                schema: "wh",
                newName: "value_adjustments",
                newSchema: "wh");

            migrationBuilder.RenameIndex(
                name: "IX_value_adjustment_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustments",
                newName: "IX_value_adjustments_WarehousePhysicalId");

            migrationBuilder.RenameIndex(
                name: "IX_value_adjustment_BinId",
                schema: "wh",
                table: "value_adjustments",
                newName: "IX_value_adjustments_BinId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "job",
                table: "export_schedule",
                type: "timestamptz",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "job",
                table: "export_job",
                type: "timestamptz",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<Guid>(
                name: "WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DeltaValue",
                schema: "wh",
                table: "value_adjustments",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)");

            migrationBuilder.AddColumn<string>(
                name: "user",
                schema: "wh",
                table: "value_adjustments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_value_adjustments",
                schema: "wh",
                table: "value_adjustments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_balance_ItemId_WarehousePhysicalId_BinId_BatchId",
                schema: "wh",
                table: "stock_balance",
                columns: new[] { "ItemId", "WarehousePhysicalId", "BinId", "BatchId" },
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustments_bin_BinId",
                schema: "wh",
                table: "value_adjustments",
                column: "BinId",
                principalSchema: "wh",
                principalTable: "bin",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustments_item_ItemId",
                schema: "wh",
                table: "value_adjustments",
                column: "ItemId",
                principalSchema: "wh",
                principalTable: "item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_value_adjustments_warehouse_physical_WarehousePhysicalId",
                schema: "wh",
                table: "value_adjustments",
                column: "WarehousePhysicalId",
                principalSchema: "wh",
                principalTable: "warehouse_physical",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
