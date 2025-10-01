using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Seed;

public static class DevSeed
{
    public static async Task EnsureAsync(WarehouseDbContext db)
    {
        var defaultLogical = await db.WarehouseLogicals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "DEFAULT");

        Guid defaultId;
        if (defaultLogical is null)
        {
            var e = new WarehouseLogical { Code = "DEFAULT", Name = "Default Pool", Kind = "DEFAULT" };
            db.WarehouseLogicals.Add(e);
            await db.SaveChangesAsync();
            defaultId = e.Id;
        }
        else
        {
            defaultId = defaultLogical.Id;
        }

        if (!await db.WarehouseLogicals.AnyAsync(x => x.Code == "QUAR"))
        {
            db.WarehouseLogicals.Add(new WarehouseLogical { Code = "QUAR", Name = "Quarantine", Kind = "CUSTOM" });
            await db.SaveChangesAsync();
        }

        if (!await db.WarehousePhysicals.AnyAsync())
        {
            db.WarehousePhysicals.AddRange(
                new WarehousePhysical { Code = "WH1", Name = "Main WH", Address = "HQ", LogicalId = defaultId },
                new WarehousePhysical { Code = "WH2", Name = "Aux WH", Address = "Branch", LogicalId = defaultId }
            );
            await db.SaveChangesAsync();
        }

        var wh1Id = await db.WarehousePhysicals.AsNoTracking()
            .Where(x => x.Code == "WH1")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (wh1Id != Guid.Empty)
        {
            var zone = await db.WarehouseZones.FirstOrDefaultAsync(z => z.WarehousePhysicalId == wh1Id && z.Code == "Z1");
            if (zone is null)
            {
                zone = new WarehouseZone { WarehousePhysicalId = wh1Id, Code = "Z1", Name = "Zone 1" };
                db.WarehouseZones.Add(zone);
                await db.SaveChangesAsync();
            }

            var rack = await db.WarehouseRacks.FirstOrDefaultAsync(r => r.WarehouseZoneId == zone.Id && r.Code == "R1");
            if (rack is null)
            {
                rack = new WarehouseRack { WarehouseZoneId = zone.Id, Code = "R1", Name = "Rack 1" };
                db.WarehouseRacks.Add(rack);
                await db.SaveChangesAsync();
            }

            if (!await db.Bins.AnyAsync(b => b.WarehousePhysicalId == wh1Id))
            {
                db.Bins.AddRange(
                    new Bin
                    {
                        WarehousePhysicalId = wh1Id,
                        WarehouseZoneId = zone.Id,
                        WarehouseRackId = rack.Id,
                        Code = "B01",
                        Kind = "STORAGE"
                    },
                    new Bin
                    {
                        WarehousePhysicalId = wh1Id,
                        WarehouseZoneId = zone.Id,
                        WarehouseRackId = rack.Id,
                        Code = "B02",
                        Kind = "STORAGE"
                    },
                    new Bin
                    {
                        WarehousePhysicalId = wh1Id,
                        Code = "RECEIVE",
                        Kind = "RECEIVE"
                    }
                );
                await db.SaveChangesAsync();
            }
        }

        if (!await db.Items.AnyAsync())
        {
            db.Items.AddRange(
                new Item { Sku = "ROLLER-01", Name = "Roller blind base", UomBase = "pcs" },
                new Item { Sku = "FABRIC-01", Name = "Fabric, white", UomBase = "m" }
            );
            await db.SaveChangesAsync();
        }
    }
}
