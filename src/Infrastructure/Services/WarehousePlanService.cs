using Lkvitai.Warehouse.Application.WarehousePlan;
using Lkvitai.Warehouse.Application.WarehousePlan.Contracts;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Services;

public class WarehousePlanService : IWarehousePlanService
{
    private readonly WarehouseDbContext _db;
    public WarehousePlanService(WarehouseDbContext db) => _db = db;

    public async Task<WarehousePlanDto?> GetPlanAsync(Guid warehouseId, CancellationToken ct = default)
    {
        var warehouse = await _db.WarehousePhysicals.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == warehouseId, ct);
        if (warehouse is null) return null;

        var zones = await _db.WarehouseZones.AsNoTracking()
            .Where(z => z.WarehousePhysicalId == warehouseId && z.IsActive)
            .OrderBy(z => z.Code)
            .ToListAsync(ct);

        var zoneIds = zones.Select(z => z.Id).ToList();

        var racks = await _db.WarehouseRacks.AsNoTracking()
            .Where(r => zoneIds.Contains(r.WarehouseZoneId) && r.IsActive)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);

        var bins = await _db.Bins.AsNoTracking()
            .Where(b => b.WarehousePhysicalId == warehouseId && b.IsActive)
            .OrderBy(b => b.Code)
            .ToListAsync(ct);

        var binIds = bins.Select(b => b.Id).ToList();

        var qtyByBin = await _db.StockBalances.AsNoTracking()
            .Where(b => b.WarehousePhysicalId == warehouseId && b.BinId != null && binIds.Contains(b.BinId.Value))
            .GroupBy(b => b.BinId!.Value)
            .Select(g => new { BinId = g.Key, Qty = g.Sum(x => x.QtyBase) })
            .ToListAsync(ct);

        var qtyLookup = qtyByBin.ToDictionary(x => x.BinId, x => x.Qty);

        var racksByZone = racks.GroupBy(r => r.WarehouseZoneId)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.Code).ToList());

        var binsByRack = bins.Where(b => b.WarehouseRackId != null)
            .GroupBy(b => b.WarehouseRackId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.Code).ToList());

        var binsByZoneWithoutRack = bins.Where(b => b.WarehouseZoneId != null && b.WarehouseRackId == null)
            .GroupBy(b => b.WarehouseZoneId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.Code).ToList());

        var planZones = new List<WarehousePlanZoneDto>();

        foreach (var zone in zones)
        {
            var rackDtos = new List<WarehousePlanRackDto>();

            if (racksByZone.TryGetValue(zone.Id, out var zoneRacks))
            {
                foreach (var rack in zoneRacks)
                {
                    var rackBins = binsByRack.TryGetValue(rack.Id, out var rb) ? rb : new List<Lkvitai.Warehouse.Domain.Entities.Bin>();

                    var binDtos = rackBins
                        .Select(b => new WarehousePlanBinDto(b.Id, b.Code, qtyLookup.TryGetValue(b.Id, out var qty) ? qty : 0m))
                        .ToList();

                    rackDtos.Add(new WarehousePlanRackDto(rack.Id, rack.Code, rack.Name, binDtos));
                }
            }

            if (binsByZoneWithoutRack.TryGetValue(zone.Id, out var zoneBins) && zoneBins.Count > 0)
            {
                var binDtos = zoneBins
                    .Select(b => new WarehousePlanBinDto(b.Id, b.Code, qtyLookup.TryGetValue(b.Id, out var qty) ? qty : 0m))
                    .ToList();
                rackDtos.Add(new WarehousePlanRackDto(zone.Id, $"{zone.Code}-ROOT", zone.Name, binDtos));
            }

            planZones.Add(new WarehousePlanZoneDto(zone.Id, zone.Code, zone.Name, rackDtos));
        }

        var unassignedBins = bins.Where(b => b.WarehouseZoneId == null)
            .OrderBy(b => b.Code)
            .ToList();

        if (unassignedBins.Count > 0)
        {
            var binDtos = unassignedBins
                .Select(b => new WarehousePlanBinDto(b.Id, b.Code, qtyLookup.TryGetValue(b.Id, out var qty) ? qty : 0m))
                .ToList();
            var rackDto = new WarehousePlanRackDto(Guid.Empty, "NO-ZONE", "Unassigned", binDtos);
            planZones.Add(new WarehousePlanZoneDto(Guid.Empty, "NO-ZONE", "Unassigned", new List<WarehousePlanRackDto> { rackDto }));
        }

        return new WarehousePlanDto(warehouse.Id, warehouse.Code, warehouse.Name, planZones);
    }

    public async Task<IEnumerable<LocateBinDto>> LocateAsync(Guid warehouseId, Guid itemId, Guid? batchId, CancellationToken ct = default)
    {
        var warehouse = await _db.WarehousePhysicals.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == warehouseId, ct);
        if (warehouse is null) throw new KeyNotFoundException("Warehouse not found");

        var balanceQuery = _db.StockBalances.AsNoTracking()
            .Where(b => b.WarehousePhysicalId == warehouseId && b.ItemId == itemId && b.BinId != null);

        if (batchId.HasValue)
            balanceQuery = balanceQuery.Where(b => b.BatchId == batchId);

        var grouped = await balanceQuery
            .GroupBy(b => b.BinId!.Value)
            .Select(g => new { BinId = g.Key, Qty = g.Sum(x => x.QtyBase) })
            .Where(x => x.Qty != 0)
            .OrderByDescending(x => x.Qty)
            .ToListAsync(ct);

        if (grouped.Count == 0)
            return Array.Empty<LocateBinDto>();

        var binIds = grouped.Select(x => x.BinId).ToList();

        var bins = await _db.Bins.AsNoTracking()
            .Where(b => binIds.Contains(b.Id))
            .Select(b => new
            {
                b.Id,
                b.Code,
                b.WarehouseZoneId,
                b.WarehouseRackId
            })
            .ToListAsync(ct);

        var zoneIds = bins.Where(b => b.WarehouseZoneId != null).Select(b => b.WarehouseZoneId!.Value).Distinct().ToList();
        var rackIds = bins.Where(b => b.WarehouseRackId != null).Select(b => b.WarehouseRackId!.Value).Distinct().ToList();

        var zones = await _db.WarehouseZones.AsNoTracking()
            .Where(z => zoneIds.Contains(z.Id))
            .Select(z => new { z.Id, z.Code })
            .ToListAsync(ct);
        var racks = await _db.WarehouseRacks.AsNoTracking()
            .Where(r => rackIds.Contains(r.Id))
            .Select(r => new { r.Id, r.Code })
            .ToListAsync(ct);

        var zoneLookup = zones.ToDictionary(z => z.Id, z => z.Code);
        var rackLookup = racks.ToDictionary(r => r.Id, r => r.Code);
        var binLookup = bins.ToDictionary(b => b.Id);

        return grouped.Select(row =>
        {
            var bin = binLookup[row.BinId];
            string? zoneCode = null;
            if (bin.WarehouseZoneId is Guid zoneId)
                zoneLookup.TryGetValue(zoneId, out zoneCode);

            string? rackCode = null;
            if (bin.WarehouseRackId is Guid rackId)
                rackLookup.TryGetValue(rackId, out rackCode);

            return new LocateBinDto(
                row.BinId,
                bin.Code,
                warehouse.Code,
                zoneCode,
                rackCode,
                row.Qty);
        }).ToList();
    }
}
