using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveColumnsOnMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ToBinId",
                schema: "wh",
                table: "movement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ToWarehousePhysicalId",
                schema: "wh",
                table: "movement",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_movement_ItemId_ToBinId_BatchId",
                schema: "wh",
                table: "movement",
                columns: new[] { "ItemId", "ToBinId", "BatchId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_movement_ItemId_ToBinId_BatchId",
                schema: "wh",
                table: "movement");

            migrationBuilder.DropColumn(
                name: "ToBinId",
                schema: "wh",
                table: "movement");

            migrationBuilder.DropColumn(
                name: "ToWarehousePhysicalId",
                schema: "wh",
                table: "movement");
        }
    }
}
