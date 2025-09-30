using FluentAssertions;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace Lkvitai.Warehouse.UnitTests;

public class MovementServiceTests
{
    [Fact]
    public async Task IN_increases_balance_and_adjust_negative_is_blocked()
    {
        await using var td = new TestDb();

        var item = new Item { Sku = "T-1", Name = "Test", UomBase = "pcs" };
        var wh = new WarehousePhysical { Code = "WH1", Name = "WH" };
        var bin = new Bin { WarehousePhysicalId = wh.Id, Code = "RECEIVE", Kind = "RECEIVE" };
        td.Db.Items.Add(item); td.Db.WarehousePhysicals.Add(wh); td.Db.Bins.Add(bin);
        await td.Db.SaveChangesAsync();

        var svc = new MovementService(td.Db);

        // IN +10
        await svc.PostAsync(new Movement
        {
            Type = "IN",
            ItemId = item.Id,
            WarehousePhysicalId = wh.Id,
            BinId = bin.Id,
            QtyBase = 10
        });

        var total = await td.Db.StockBalances.SumAsync(x => x.QtyBase);
        total.Should().Be(10);

        // ADJUST -11 -> should throw (negative balance)
        var act = () => svc.PostAsync(new Movement
        {
            Type = "ADJUST",
            ItemId = item.Id,
            WarehousePhysicalId = wh.Id,
            BinId = bin.Id,
            QtyBase = -11
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public async Task MOVE_moves_qty_between_bins_atomically()
    {
        await using var td = new TestDb();

        var item = new Item { Sku = "T-2", Name = "Test2", UomBase = "pcs" };
        var wh = new WarehousePhysical { Code = "WH1", Name = "WH" };
        var src = new Bin { WarehousePhysicalId = wh.Id, Code = "SRC", Kind = "STORAGE" };
        var dst = new Bin { WarehousePhysicalId = wh.Id, Code = "DST", Kind = "STORAGE" };
        td.Db.AddRange(item, wh, src, dst);
        await td.Db.SaveChangesAsync();

        var svc = new MovementService(td.Db);

        await svc.PostAsync(new Movement { Type = "IN", ItemId = item.Id, WarehousePhysicalId = wh.Id, BinId = src.Id, QtyBase = 8 });

        await svc.PostAsync(new Movement
        {
            Type = "MOVE",
            ItemId = item.Id,
            WarehousePhysicalId = wh.Id,
            BinId = src.Id,
            ToWarehousePhysicalId = wh.Id,
            ToBinId = dst.Id,
            QtyBase = 5
        });

        var srcQty = await td.Db.StockBalances.Where(b => b.BinId == src.Id).SumAsync(b => b.QtyBase);
        var dstQty = await td.Db.StockBalances.Where(b => b.BinId == dst.Id).SumAsync(b => b.QtyBase);

        srcQty.Should().Be(3);
        dstQty.Should().Be(5);
    }
}
