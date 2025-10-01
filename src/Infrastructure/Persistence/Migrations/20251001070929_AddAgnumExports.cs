using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgnumExports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "job");

            migrationBuilder.CreateTable(
                name: "export_job",
                schema: "job",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SliceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SliceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ErrorFilePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExportedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_export_job", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "export_schedule",
                schema: "job",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SliceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SliceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Cron = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastRunAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    NextRunAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_export_schedule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_export_job_SliceType_SliceKey",
                schema: "job",
                table: "export_job",
                columns: new[] { "SliceType", "SliceKey" });

            migrationBuilder.CreateIndex(
                name: "IX_export_job_Status",
                schema: "job",
                table: "export_job",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_export_schedule_Enabled",
                schema: "job",
                table: "export_schedule",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_export_schedule_SliceType_SliceKey",
                schema: "job",
                table: "export_schedule",
                columns: new[] { "SliceType", "SliceKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "export_job",
                schema: "job");

            migrationBuilder.DropTable(
                name: "export_schedule",
                schema: "job");
        }
    }
}
