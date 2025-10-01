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
                name: "value_adjustment",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePhysicalId = table.Column<Guid>(type: "uuid", nullable: true),
                    BinId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeltaValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    performed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_value_adjustment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustment_ItemId_BatchId_WarehousePhysicalId_BinId",
                schema: "wh",
                table: "value_adjustment",
                columns: new[] { "ItemId", "BatchId", "WarehousePhysicalId", "BinId" });

            migrationBuilder.CreateIndex(
                name: "IX_value_adjustment_Timestamp",
                schema: "wh",
                table: "value_adjustment",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "value_adjustment",
                schema: "wh");
        }
    }
}
