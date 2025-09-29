using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Seed;

public static class DevSeed
{
    public static async Task EnsureAsync(WarehouseDbContext db)
    {
        // На всякий случай — применим миграции (можно убрать, если делаешь отдельно)
        // await db.Database.MigrateAsync();

        // 1) Логический DEFAULT — найти или создать
        var defaultLogical = await db.WarehouseLogicals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "DEFAULT");

        Guid defaultId;
        if (defaultLogical is null)
        {
            var e = new WarehouseLogical { Code = "DEFAULT", Name = "Default Pool", Kind = "DEFAULT" };
            db.WarehouseLogicals.Add(e);
            await db.SaveChangesAsync();
            defaultId = e.Id; // Id уже есть после SaveChanges
        }
        else
        {
            defaultId = defaultLogical.Id;
        }

        // 2) Доп. логический склад QUAR — создать если нет
        if (!await db.WarehouseLogicals.AnyAsync(x => x.Code == "QUAR"))
        {
            db.WarehouseLogicals.Add(new WarehouseLogical { Code = "QUAR", Name = "Quarantine", Kind = "CUSTOM" });
            await db.SaveChangesAsync();
        }

        // 3) Физ. склады — привязать к defaultId
        if (!await db.WarehousePhysicals.AnyAsync())
        {
            db.WarehousePhysicals.AddRange(
                new WarehousePhysical { Code = "WH1", Name = "Main WH", Address = "HQ",    LogicalId = defaultId },
                new WarehousePhysical { Code = "WH2", Name = "Aux WH",  Address = "Branch", LogicalId = defaultId }
            );
            await db.SaveChangesAsync();
        }

        // 4) Ячейки для WH1
        var wh1Id = await db.WarehousePhysicals.AsNoTracking()
            .Where(x => x.Code == "WH1")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (wh1Id != Guid.Empty && !await db.Bins.AnyAsync(b => b.WarehousePhysicalId == wh1Id))
        {
            db.Bins.AddRange(
                new Bin { WarehousePhysicalId = wh1Id, Code = "Z1-R1-B01", Kind = "STORAGE" },
                new Bin { WarehousePhysicalId = wh1Id, Code = "RECEIVE",   Kind = "RECEIVE" }
            );
            await db.SaveChangesAsync();
        }

        // 5) Пара номенклатур
        if (!await db.Items.AnyAsync())
        {
            db.Items.AddRange(
                new Item { Sku = "ROLLER-01", Name = "Roller blind base", UomBase = "pcs" },
                new Item { Sku = "FABRIC-01", Name = "Fabric, white",     UomBase = "m"   }
            );
            await db.SaveChangesAsync();
        }
    }
}
