using Xunit;
using Lkvitai.Warehouse.Application.Exports.Contracts;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Options;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lkvitai.Warehouse.UnitTests;

public sealed class AgnumExportServiceTests
{
    [Fact]
    public async Task CSV_contains_required_columns_and_values()
    {
        await using var testDb = new TestDb();
        var db = testDb.Db;

        var item = new Item { Sku = "SKU-1", Name = "Item", UomBase = "PCS" };
        var warehouse = new WarehousePhysical { Code = "WH1", Name = "Warehouse" };
        var bin = new Bin { WarehousePhysicalId = warehouse.Id, Code = "B01", Kind = "STORAGE" };

        db.Items.Add(item);
        db.WarehousePhysicals.Add(warehouse);
        db.Bins.Add(bin);

        db.StockBalances.Add(new StockBalance
        {
            ItemId = item.Id,
            WarehousePhysicalId = warehouse.Id,
            BinId = bin.Id,
            QtyBase = 10m
        });

        db.ValueAdjustments.Add(new ValueAdjustment
        {
            ItemId = item.Id,
            WarehousePhysicalId = warehouse.Id,
            BinId = bin.Id,
            DeltaValue = 5m,
            Reason = "Manual"
        });

        await db.SaveChangesAsync();

        var options = CreateOptions();
        var service = new AgnumExportService(db, options, NullLogger<AgnumExportService>.Instance);

        var job = await service.RunAsync(new AgnumExportRequest(AgnumExportRequest.Types.Physical, warehouse.Code));

        Assert.Equal(ExportJobStatus.Succeeded, job.Status);
        Assert.NotNull(job.FilePath);
        Assert.True(File.Exists(job.FilePath!));

        var lines = await File.ReadAllLinesAsync(job.FilePath!);
        Assert.Equal("ExportAtUtc,SliceType,SliceKey,ItemCode,BaseUoM,QtyBase,AdjValue,BatchCode,LocationCode,Quality,ExpDate", lines[0]);
        Assert.Equal(2, lines.Length);

        var parts = lines[1].Split(',');
        Assert.Equal("Physical", parts[1]);
        Assert.Equal("WH1-B01", parts[2]);
        Assert.Equal("SKU-1", parts[3]);
        Assert.Equal("PCS", parts[4]);
        Assert.Equal("10", parts[5]);
        Assert.Equal("5.00", parts[6]);
        Assert.Equal("B01", parts[8]);

        Cleanup(options.Value);
    }

    [Fact]
    public async Task Total_slice_aggregates_bins()
    {
        await using var testDb = new TestDb();
        var db = testDb.Db;

        var item = new Item { Sku = "SKU-2", Name = "Item", UomBase = "PCS" };
        var warehouse = new WarehousePhysical { Code = "WH1", Name = "Warehouse" };
        var binA = new Bin { WarehousePhysicalId = warehouse.Id, Code = "A1" };
        var binB = new Bin { WarehousePhysicalId = warehouse.Id, Code = "B2" };

        db.Items.Add(item);
        db.WarehousePhysicals.Add(warehouse);
        db.Bins.AddRange(binA, binB);

        db.StockBalances.AddRange(
            new StockBalance { ItemId = item.Id, WarehousePhysicalId = warehouse.Id, BinId = binA.Id, QtyBase = 3m },
            new StockBalance { ItemId = item.Id, WarehousePhysicalId = warehouse.Id, BinId = binB.Id, QtyBase = 5m });

        db.ValueAdjustments.AddRange(
            new ValueAdjustment { ItemId = item.Id, WarehousePhysicalId = warehouse.Id, BinId = binA.Id, DeltaValue = 1m, Reason = "Adj" },
            new ValueAdjustment { ItemId = item.Id, WarehousePhysicalId = warehouse.Id, BinId = binB.Id, DeltaValue = 2m, Reason = "Adj" });

        await db.SaveChangesAsync();

        var options = CreateOptions();
        var service = new AgnumExportService(db, options, NullLogger<AgnumExportService>.Instance);

        var job = await service.RunAsync(new AgnumExportRequest(AgnumExportRequest.Types.Total, warehouse.Code));

        Assert.Equal(ExportJobStatus.Succeeded, job.Status);
        Assert.NotNull(job.FilePath);
        Assert.True(File.Exists(job.FilePath!));

        var lines = await File.ReadAllLinesAsync(job.FilePath!);
        Assert.Equal(2, lines.Length);

        var parts = lines[1].Split(',');
        Assert.Equal("Total", parts[1]);
        Assert.Equal("WH1", parts[2]);
        Assert.Equal("8", parts[5]);
        Assert.Equal("3.00", parts[6]);
        Assert.Equal(string.Empty, parts[8]);

        Cleanup(options.Value);
    }

    private static IOptions<AgnumExportOptions> CreateOptions()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "agnum-tests", Guid.NewGuid().ToString());
        var opts = new AgnumExportOptions
        {
            OutDir = Path.Combine(baseDir, "out"),
            ArchiveDir = Path.Combine(baseDir, "archive"),
            ErrorsDir = Path.Combine(baseDir, "errors"),
            CopyToArchive = false
        };

        return Options.Create(opts);
    }

    private static void Cleanup(AgnumExportOptions opts)
    {
        var root = Directory.GetParent(opts.OutDir)?.FullName;
        if (root is null || !Directory.Exists(root))
        {
            return;
        }

        try
        {
            Directory.Delete(root, recursive: true);
        }
        catch
        {
            // ignore cleanup failures in tests
        }
    }
}
