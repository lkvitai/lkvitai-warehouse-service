using FluentAssertions;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace Lkvitai.Warehouse.UnitTests;

public class InventoryServiceTests
{
    [Fact]
    public async Task Post_is_idempotent_and_creates_adjust_when_delta_exists()
    {
        await using var td = new TestDb();

        var item = new Item { Sku = "I-1", Name = "Inv", UomBase = "pcs" };
        var wh = new WarehousePhysical { Code = "WH1", Name = "WH" };
        var bin = new Bin { WarehousePhysicalId = wh.Id, Code = "B1", Kind = "STORAGE" };
        td.Db.AddRange(item, wh, bin);
        await td.Db.SaveChangesAsync();

        var moveSvc = new MovementService(td.Db);
        var invSvc = new InventoryService(td.Db, moveSvc);

        // system has 3 pcs
        await moveSvc.PostAsync(new Movement { Type = "IN", ItemId = item.Id, WarehousePhysicalId = wh.Id, BinId = bin.Id, QtyBase = 3 });

        var s = await invSvc.OpenAsync(wh.Id, "INV-TEST");
        await invSvc.AddCountAsync(s.Id, item.Id, bin.Id, 2, "u1"); // delta -1

        var first = await invSvc.PostAsync(s.Id);
        first.Should().HaveCount(1);
        var total = await td.Db.StockBalances.Where(b => b.BinId == bin.Id).SumAsync(b => b.QtyBase);
        total.Should().Be(2);

        var second = await invSvc.PostAsync(s.Id);
        second.Should().BeEmpty(); // idempotent
    }
}
