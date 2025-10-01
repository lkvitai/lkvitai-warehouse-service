using System.Linq;
using FluentAssertions;
using Lkvitai.Warehouse.Application.Batches.Contracts;
using Lkvitai.Warehouse.Infrastructure.Services;
using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lkvitai.Warehouse.UnitTests;

public class BatchTraceabilityTests
{
    [Fact]
    public async Task Movement_service_creates_stock_batch_on_in()
    {
        await using var td = new TestDb();

        var item = new Item { Sku = "BATCH-01", Name = "Batch item", UomBase = "kg" };
        var warehouse = new WarehousePhysical { Code = "WH1", Name = "Main" };
        var bin = new Bin { WarehousePhysicalId = warehouse.Id, Code = "B01", Kind = "STORAGE" };
        td.Db.AddRange(item, warehouse, bin);
        await td.Db.SaveChangesAsync();

        var svc = new MovementService(td.Db);
        var batchId = Guid.NewGuid();
        var movement = new Movement
        {
            Type = "IN",
            ItemId = item.Id,
            WarehousePhysicalId = warehouse.Id,
            BinId = bin.Id,
            QtyBase = 12.5m,
            BatchId = batchId,
            BatchNo = "LOT-123",
            BatchQuality = "OK"
        };

        await svc.PostAsync(movement);

        var storedBatch = await td.Db.StockBatches.FindAsync(batchId);
        storedBatch.Should().NotBeNull();
        storedBatch!.ItemId.Should().Be(item.Id);
        storedBatch.BatchNo.Should().Be("LOT-123");

        var balance = await td.Db.StockBalances.SingleAsync(b => b.BatchId == batchId);
        balance.QtyBase.Should().Be(12.5m);
    }

    [Fact]
    public async Task Batch_trace_lists_movements_and_locations()
    {
        await using var td = new TestDb();

        var item = new Item { Sku = "FAB-01", Name = "Fabric", UomBase = "m" };
        var warehouse = new WarehousePhysical { Code = "WH1", Name = "Main" };
        var zone = new WarehouseZone { WarehousePhysicalId = warehouse.Id, Code = "Z1", Name = "Zone 1" };
        var rack = new WarehouseRack { WarehouseZoneId = zone.Id, Code = "R1", Name = "Rack 1" };
        var binSrc = new Bin
        {
            WarehousePhysicalId = warehouse.Id,
            WarehouseZoneId = zone.Id,
            WarehouseRackId = rack.Id,
            Code = "B01",
            Kind = "STORAGE"
        };
        var binDst = new Bin
        {
            WarehousePhysicalId = warehouse.Id,
            WarehouseZoneId = zone.Id,
            WarehouseRackId = rack.Id,
            Code = "B02",
            Kind = "STORAGE"
        };
        td.Db.AddRange(item, warehouse, zone, rack, binSrc, binDst);
        await td.Db.SaveChangesAsync();

        var movementSvc = new MovementService(td.Db);
        var batchId = Guid.NewGuid();

        await movementSvc.PostAsync(new Movement
        {
            Type = "IN",
            ItemId = item.Id,
            WarehousePhysicalId = warehouse.Id,
            BinId = binSrc.Id,
            QtyBase = 10m,
            BatchId = batchId,
            BatchNo = "LOT-900"
        });

        await movementSvc.PostAsync(new Movement
        {
            Type = "MOVE",
            ItemId = item.Id,
            WarehousePhysicalId = warehouse.Id,
            BinId = binSrc.Id,
            ToWarehousePhysicalId = warehouse.Id,
            ToBinId = binDst.Id,
            QtyBase = 4m,
            BatchId = batchId
        });

        var querySvc = new BatchQueryService(td.Db);
        var trace = await querySvc.GetTraceAsync(batchId);

        trace.Should().NotBeNull();
        trace!.BatchId.Should().Be(batchId);
        trace.Movements.Should().HaveCount(2);
        trace.Movements.Select(m => m.Type).Should().ContainInOrder("IN", "MOVE");

        trace.Locations.Should().NotBeEmpty();
        var locationPaths = trace.Locations.Select(l => l.Path).ToList();
        locationPaths.Should().Contain(path => path.Contains("WH1"));

        var details = await querySvc.GetDetailsAsync(batchId);
        details!.Locations.Should().HaveCount(2);
        details.Locations.Sum(l => l.QtyBase).Should().Be(10m);
    }
}
