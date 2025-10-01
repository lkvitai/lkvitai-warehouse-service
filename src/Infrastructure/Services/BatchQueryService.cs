using Lkvitai.Warehouse.Application.Batches;
using Lkvitai.Warehouse.Application.Batches.Contracts;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Services;

public class BatchQueryService : IBatchQueryService
{
    private readonly WarehouseDbContext _db;
    public BatchQueryService(WarehouseDbContext db) => _db = db;

    public async Task<BatchDetailsDto?> GetDetailsAsync(Guid batchId, CancellationToken ct = default)
    {
        var batch = await _db.StockBatches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == batchId, ct);
        if (batch is null) return null;

        var item = await _db.Items.AsNoTracking()
            .Where(i => i.Id == batch.ItemId)
            .Select(i => new { i.Sku, i.Name })
            .FirstOrDefaultAsync(ct);
        if (item is null)
            throw new InvalidOperationException("Batch references missing item.");

        var balances = await _db.StockBalances.AsNoTracking()
            .Where(b => b.BatchId == batchId)
            .ToListAsync(ct);

        var warehouseIds = balances.Where(b => b.WarehousePhysicalId != null)
            .Select(b => b.WarehousePhysicalId!.Value)
            .Distinct()
            .ToList();
        var binIds = balances.Where(b => b.BinId != null)
            .Select(b => b.BinId!.Value)
            .Distinct()
            .ToList();

        var warehouses = await _db.WarehousePhysicals.AsNoTracking()
            .Where(w => warehouseIds.Contains(w.Id))
            .Select(w => new { w.Id, w.Code })
            .ToListAsync(ct);
        var bins = await _db.Bins.AsNoTracking()
            .Where(b => binIds.Contains(b.Id))
            .Select(b => new { b.Id, b.Code, b.WarehouseZoneId, b.WarehouseRackId })
            .ToListAsync(ct);

        var zoneIds = bins.Where(b => b.WarehouseZoneId != null)
            .Select(b => b.WarehouseZoneId!.Value)
            .Distinct()
            .ToList();
        var rackIds = bins.Where(b => b.WarehouseRackId != null)
            .Select(b => b.WarehouseRackId!.Value)
            .Distinct()
            .ToList();

        var zones = await _db.WarehouseZones.AsNoTracking()
            .Where(z => zoneIds.Contains(z.Id))
            .Select(z => new { z.Id, z.Code })
            .ToListAsync(ct);
        var racks = await _db.WarehouseRacks.AsNoTracking()
            .Where(r => rackIds.Contains(r.Id))
            .Select(r => new { r.Id, r.Code })
            .ToListAsync(ct);

        var warehouseLookup = warehouses.ToDictionary(w => w.Id, w => w.Code);
        var binLookup = bins.ToDictionary(b => b.Id);
        var zoneLookup = zones.ToDictionary(z => z.Id, z => z.Code);
        var rackLookup = racks.ToDictionary(r => r.Id, r => r.Code);

        var locations = balances
            .OrderByDescending(b => b.QtyBase)
            .Select(b =>
            {
                string? warehouseCode = null;
                if (b.WarehousePhysicalId is Guid warehouseId)
                    warehouseLookup.TryGetValue(warehouseId, out warehouseCode);

                string? binCode = null;
                string? zoneCode = null;
                string? rackCode = null;

                if (b.BinId is Guid binId && binLookup.TryGetValue(binId, out var bin))
                {
                    binCode = bin.Code;
                    if (bin.WarehouseZoneId is Guid zoneId)
                        zoneLookup.TryGetValue(zoneId, out zoneCode);
                    if (bin.WarehouseRackId is Guid rackId)
                        rackLookup.TryGetValue(rackId, out rackCode);
                }

                return new BatchLocationDto(
                    b.WarehousePhysicalId,
                    warehouseCode ?? string.Empty,
                    b.BinId,
                    binCode,
                    zoneCode,
                    rackCode,
                    b.QtyBase);
            })
            .ToList();

        return new BatchDetailsDto(
            batch.Id,
            batch.ItemId,
            item.Sku,
            item.Name,
            batch.BatchNo,
            batch.MfgDate,
            batch.ExpDate,
            batch.Quality.ToString(),
            locations);
    }

    public async Task<BatchTraceDto?> GetTraceAsync(Guid batchId, CancellationToken ct = default)
    {
        var details = await GetDetailsAsync(batchId, ct);
        if (details is null) return null;

        var movements = await _db.Movements.AsNoTracking()
            .Where(m => m.BatchId == batchId)
            .Select(m => new BatchTraceMovementDto(
                m.Id,
                m.Type,
                m.QtyBase,
                m.PerformedAt,
                m.WarehousePhysicalId,
                m.BinId,
                m.ToWarehousePhysicalId,
                m.ToBinId))
            .ToListAsync(ct);

        var orderedMovements = movements
            .OrderBy(m => m.PerformedAt)
            .ToList();

        var locationPaths = BuildLocationPaths(details.Locations);

        return new BatchTraceDto(
            batchId,
            details.Sku,
            orderedMovements,
            locationPaths);
    }

    private static IReadOnlyList<BatchTraceLocationDto> BuildLocationPaths(IEnumerable<BatchLocationDto> locations)
    {
        return locations
            .Where(l => l.QtyBase != 0)
            .Select(l =>
            {
                var segments = new List<string>();
                if (!string.IsNullOrWhiteSpace(l.WarehouseCode)) segments.Add(l.WarehouseCode);
                if (!string.IsNullOrWhiteSpace(l.ZoneCode)) segments.Add(l.ZoneCode!);
                if (!string.IsNullOrWhiteSpace(l.RackCode)) segments.Add(l.RackCode!);
                if (!string.IsNullOrWhiteSpace(l.BinCode)) segments.Add(l.BinCode!);
                var path = string.Join('-', segments);
                return new BatchTraceLocationDto(path, l.QtyBase);
            })
            .ToList();
    }
}
