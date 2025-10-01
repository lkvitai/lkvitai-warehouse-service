using Lkvitai.Warehouse.Application.ValueAdjustments;
using Lkvitai.Warehouse.Application.ValueAdjustments.Contracts;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Services;

public sealed class ValueAdjustmentService : IValueAdjustmentService
{
    private readonly WarehouseDbContext _db;

    public ValueAdjustmentService(WarehouseDbContext db) => _db = db;

    public async Task<ValueAdjustmentDto> CreateAsync(CreateValueAdjustmentRequest request, CancellationToken ct)
    {
        if (request.DeltaValue == 0m)
            throw new ArgumentException("DeltaValue must be non-zero.", nameof(request));

        if (request.ItemId == Guid.Empty)
            throw new ArgumentException("ItemId is required.", nameof(request));

        var reason = request.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(request));

        var timestamp = (request.Timestamp ?? DateTimeOffset.UtcNow).ToUniversalTime();

        var userId = string.IsNullOrWhiteSpace(request.UserId) ? null : request.UserId.Trim();

        var itemExists = await _db.Items.AsNoTracking().AnyAsync(x => x.Id == request.ItemId, ct);
        if (!itemExists) throw new InvalidOperationException("Item not found.");

        Guid? warehouseId = null;
        if (request.WarehousePhysicalId.HasValue)
        {
            if (request.WarehousePhysicalId.Value == Guid.Empty)
                throw new ArgumentException("WarehousePhysicalId must be a valid identifier when specified.", nameof(request));

            warehouseId = request.WarehousePhysicalId.Value;
            var whExists = await _db.WarehousePhysicals.AsNoTracking().AnyAsync(x => x.Id == warehouseId, ct);
            if (!whExists) throw new InvalidOperationException("Warehouse not found.");
        }

        Guid? logicalId = null;
        if (request.WarehouseLogicalId.HasValue)
        {
            if (request.WarehouseLogicalId.Value == Guid.Empty)
                throw new ArgumentException("WarehouseLogicalId must be a valid identifier when specified.", nameof(request));

            logicalId = request.WarehouseLogicalId.Value;
            var logicalExists = await _db.WarehouseLogicals.AsNoTracking().AnyAsync(x => x.Id == logicalId, ct);
            if (!logicalExists) throw new InvalidOperationException("Logical warehouse not found.");
        }

        Guid? binId = null;
        if (request.BinId.HasValue)
        {
            if (request.BinId.Value == Guid.Empty)
                throw new ArgumentException("BinId must be a valid identifier when specified.", nameof(request));

            binId = request.BinId.Value;
            var binExists = await _db.Bins.AsNoTracking().AnyAsync(x => x.Id == binId, ct);
            if (!binExists) throw new InvalidOperationException("Bin not found.");
        }

        Guid? batchId = null;
        if (request.BatchId.HasValue)
        {
            if (request.BatchId.Value == Guid.Empty)
                throw new ArgumentException("BatchId must be a valid identifier when specified.", nameof(request));

            batchId = request.BatchId.Value;
            var batchExists = await _db.StockBatches.AsNoTracking().AnyAsync(x => x.Id == batchId, ct);
            if (!batchExists) throw new InvalidOperationException("Batch not found.");
        }

        var entity = new ValueAdjustment
        {
            ItemId = request.ItemId,
            WarehousePhysicalId = warehouseId,
            WarehouseLogicalId = logicalId,
            BinId = binId,
            BatchId = batchId,
            DeltaValue = request.DeltaValue,
            Reason = reason!,
            Timestamp = timestamp,
            UserId = userId
        };

        _db.ValueAdjustments.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<IReadOnlyList<ValueAdjustmentDto>> GetHistoryAsync(
        Guid itemId,
        Guid? warehousePhysicalId,
        Guid? warehouseLogicalId,
        Guid? binId,
        Guid? batchId,
        CancellationToken ct)
    {
        if (itemId == Guid.Empty)
            throw new ArgumentException("itemId is required.", nameof(itemId));

        var query = _db.ValueAdjustments
            .AsNoTracking()
            .Where(x => x.ItemId == itemId);

        if (warehousePhysicalId.HasValue)
            query = query.Where(x => x.WarehousePhysicalId == warehousePhysicalId.Value);
        if (warehouseLogicalId.HasValue)
            query = query.Where(x => x.WarehouseLogicalId == warehouseLogicalId.Value);
        if (binId.HasValue)
            query = query.Where(x => x.BinId == binId.Value);
        if (batchId.HasValue)
            query = query.Where(x => x.BatchId == batchId.Value);

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new ValueAdjustmentDto(
                x.Id,
                x.ItemId,
                x.WarehousePhysicalId,
                x.WarehouseLogicalId,
                x.BinId,
                x.BatchId,
                x.DeltaValue,
                x.Reason,
                x.Timestamp,
                x.UserId))
            .ToListAsync(ct);
    }

    private static ValueAdjustmentDto ToDto(ValueAdjustment entity) => new(
        entity.Id,
        entity.ItemId,
        entity.WarehousePhysicalId,
        entity.WarehouseLogicalId,
        entity.BinId,
        entity.BatchId,
        entity.DeltaValue,
        entity.Reason,
        entity.Timestamp,
        entity.UserId);
}
