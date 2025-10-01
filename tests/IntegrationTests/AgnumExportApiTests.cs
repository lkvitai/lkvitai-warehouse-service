using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Lkvitai.Warehouse.Application.Exports.Contracts;
using Lkvitai.Warehouse.Infrastructure.Options;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Lkvitai.Warehouse.IntegrationTests;

public sealed class AgnumExportApiTests : IClassFixture<TestingWebAppFactory>
{
    private readonly TestingWebAppFactory _factory;

    public AgnumExportApiTests(TestingWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RunEndpoint_creates_export_job()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "agnum-int", Guid.NewGuid().ToString());
        Directory.CreateDirectory(baseDir);
        Environment.SetEnvironmentVariable("Exports__Agnum__OutDir", Path.Combine(baseDir, "out"));
        Environment.SetEnvironmentVariable("Exports__Agnum__ArchiveDir", Path.Combine(baseDir, "archive"));
        Environment.SetEnvironmentVariable("Exports__Agnum__ErrorsDir", Path.Combine(baseDir, "errors"));
        Environment.SetEnvironmentVariable("Exports__Agnum__CopyToArchive", "false");

        try
        {
            var client = _factory.CreateClient();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();

                var item = new Domain.Entities.Item { Sku = "SKU-API", Name = "Item", UomBase = "PCS" };
                var warehouse = new Domain.Entities.WarehousePhysical { Code = "WHAPI", Name = "Warehouse" };
                var bin = new Domain.Entities.Bin { WarehousePhysicalId = warehouse.Id, Code = "BIN1" };

                db.Items.Add(item);
                db.WarehousePhysicals.Add(warehouse);
                db.Bins.Add(bin);

                db.StockBalances.Add(new Domain.Entities.StockBalance
                {
                    ItemId = item.Id,
                    WarehousePhysicalId = warehouse.Id,
                    BinId = bin.Id,
                    QtyBase = 4m
                });

                await db.SaveChangesAsync();
            }

            var response = await client.PostAsJsonAsync("/api/exports/agnum/run", new AgnumExportRequest(AgnumExportRequest.Types.Physical, "WHAPI"));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payload = await response.Content.ReadFromJsonAsync<ExportJobResponse>();
            payload.Should().NotBeNull();
            payload!.Status.Should().Be("Succeeded");
            payload.FilePath.Should().NotBeNullOrEmpty();
            File.Exists(payload.FilePath!).Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("Exports__Agnum__OutDir", null);
            Environment.SetEnvironmentVariable("Exports__Agnum__ArchiveDir", null);
            Environment.SetEnvironmentVariable("Exports__Agnum__ErrorsDir", null);
            Environment.SetEnvironmentVariable("Exports__Agnum__CopyToArchive", null);

            if (Directory.Exists(baseDir))
            {
                Directory.Delete(baseDir, recursive: true);
            }
        }
    }

    private sealed record ExportJobResponse(Guid Id, string SliceType, string SliceKey, string Format, string Status, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt, string? FilePath, string? ErrorFilePath, string? ErrorMessage);
}
