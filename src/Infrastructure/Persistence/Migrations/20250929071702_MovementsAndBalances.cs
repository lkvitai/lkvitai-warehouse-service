using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MovementsAndBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movement",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DocNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePhysicalId = table.Column<Guid>(type: "uuid", nullable: true),
                    BinId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    QtyBase = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Uom = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Factor = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    Ref = table.Column<string>(type: "text", nullable: true),
                    MetaJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stock_balance",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePhysicalId = table.Column<Guid>(type: "uuid", nullable: true),
                    BinId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    QtyBase = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_balance", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_movement_ItemId_BinId_BatchId",
                schema: "wh",
                table: "movement",
                columns: new[] { "ItemId", "BinId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_movement_PerformedAt",
                schema: "wh",
                table: "movement",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_stock_balance_ItemId_WarehousePhysicalId_BinId_BatchId",
                schema: "wh",
                table: "stock_balance",
                columns: new[] { "ItemId", "WarehousePhysicalId", "BinId", "BatchId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movement",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "stock_balance",
                schema: "wh");
        }
    }
}
