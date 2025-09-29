using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitWh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wh");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "item",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UomBase = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AttrsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_logical",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Tags = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MetaJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse_logical", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_physical",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LogicalId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MetaJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse_physical", x => x.Id);
                    table.ForeignKey(
                        name: "FK_warehouse_physical_warehouse_logical_LogicalId",
                        column: x => x.LogicalId,
                        principalSchema: "wh",
                        principalTable: "warehouse_logical",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bin",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    WarehousePhysicalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MetaJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bin_warehouse_physical_WarehousePhysicalId",
                        column: x => x.WarehousePhysicalId,
                        principalSchema: "wh",
                        principalTable: "warehouse_physical",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bin_WarehousePhysicalId_Code",
                schema: "wh",
                table: "bin",
                columns: new[] { "WarehousePhysicalId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_Sku",
                schema: "wh",
                table: "item",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_logical_Code",
                schema: "wh",
                table: "warehouse_logical",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_physical_Code",
                schema: "wh",
                table: "warehouse_physical",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_physical_LogicalId",
                schema: "wh",
                table: "warehouse_physical",
                column: "LogicalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bin",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "item",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "warehouse_physical",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "warehouse_logical",
                schema: "wh");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");
        }
    }
}
