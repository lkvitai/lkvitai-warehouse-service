using FluentAssertions;
using Lkvitai.Warehouse.Api.Controllers;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lkvitai.Warehouse.UnitTests;

public class ValueAdjustmentServiceTests
{
    [Fact]
    public async Task CreateAsync_persists_adjustment_and_keeps_stock_qty_intact()
    {
        await using var td = new TestDb();

        var item = new Item { Sku = "VAL-1", Name = "Value item", UomBase = "pcs" };
        var wh = new WarehousePhysical { Code = "WHV", Name = "Value WH" };
        var bin = new Bin { WarehousePhysicalId = wh.Id, Code = "BIN", Kind = "STORAGE" };
        var balance = new StockBalance
        {
            ItemId = item.Id,
            WarehousePhysicalId = wh.Id,
            BinId = bin.Id,
            QtyBase = 25m,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        td.Db.AddRange(item, wh, bin, balance);
        await td.Db.SaveChangesAsync();

        var svc = new ValueAdjustmentService(td.Db);

        var saved = await svc.CreateAsync(new ValueAdjustment
        {
            ItemId = item.Id,
            WarehousePhysicalId = wh.Id,
            BinId = bin.Id,
            DeltaValue = 150.50m,
            Reason = "Revaluation",
            PerformedBy = "tester"
        });

        saved.Id.Should().NotBeEmpty();
        saved.Timestamp.Should().NotBe(default);

        var dbAdjustment = await td.Db.ValueAdjustments.SingleAsync();
        dbAdjustment.PerformedBy.Should().Be("tester");
        dbAdjustment.DeltaValue.Should().Be(150.50m);

        var qty = await td.Db.StockBalances.Where(b => b.Id == balance.Id).Select(b => b.QtyBase).SingleAsync();
        qty.Should().Be(25m);
    }

    [Fact]
    public async Task Balances_controller_returns_adj_value_sum()
    {
        await using var td = new TestDb();

        var item = new Item { Sku = "VAL-2", Name = "Value item", UomBase = "pcs" };
        var wh = new WarehousePhysical { Code = "WHV2", Name = "Value WH" };
        var bin = new Bin { WarehousePhysicalId = wh.Id, Code = "B1", Kind = "STORAGE" };
        td.Db.AddRange(item, wh, bin);
        await td.Db.SaveChangesAsync();

        td.Db.StockBalances.Add(new StockBalance
        {
            ItemId = item.Id,
            WarehousePhysicalId = wh.Id,
            BinId = bin.Id,
            QtyBase = 10m,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        td.Db.ValueAdjustments.AddRange(
            new ValueAdjustment
            {
                ItemId = item.Id,
                WarehousePhysicalId = wh.Id,
                BinId = bin.Id,
                DeltaValue = 50m,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            new ValueAdjustment
            {
                ItemId = item.Id,
                WarehousePhysicalId = wh.Id,
                BinId = bin.Id,
                DeltaValue = -5m,
                Timestamp = DateTimeOffset.UtcNow
            },
            new ValueAdjustment
            {
                ItemId = item.Id,
                WarehousePhysicalId = wh.Id,
                BinId = Guid.NewGuid(),
                DeltaValue = 999m,
                Timestamp = DateTimeOffset.UtcNow
            });
        await td.Db.SaveChangesAsync();

        var controller = new BalancesController(td.Db);
        var balances = (await controller.Get(itemId: item.Id)).ToList();

        balances.Should().HaveCount(1);
        balances[0].AdjValue.Should().Be(45m);
        balances[0].QtyBase.Should().Be(10m);
    }
}
