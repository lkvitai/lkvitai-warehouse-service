using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InventorySessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_count",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    BinId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    QtyCounted = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    QtySystemAtStart = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    CountedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CountedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_count", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_session",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WarehousePhysicalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_session", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_count",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "inventory_session",
                schema: "wh");
        }
    }
}
