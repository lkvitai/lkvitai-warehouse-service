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

        if (request.WarehousePhysicalId == Guid.Empty)
            throw new ArgumentException("WarehousePhysicalId is required.", nameof(request));

        var reason = request.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(request));

        var user = string.IsNullOrWhiteSpace(request.User) ? "system" : request.User.Trim();

        var itemExists = await _db.Items.AsNoTracking().AnyAsync(x => x.Id == request.ItemId, ct);
        if (!itemExists) throw new InvalidOperationException("Item not found.");

        var whExists = await _db.WarehousePhysicals.AsNoTracking().AnyAsync(x => x.Id == request.WarehousePhysicalId, ct);
        if (!whExists) throw new InvalidOperationException("Warehouse not found.");

        if (request.BinId is Guid binId)
        {
            var binExists = await _db.Bins.AsNoTracking().AnyAsync(x => x.Id == binId, ct);
            if (!binExists) throw new InvalidOperationException("Bin not found.");
        }

        var timestamp = (request.Timestamp ?? DateTimeOffset.UtcNow).ToUniversalTime();

        var entity = new ValueAdjustment
        {
            ItemId = request.ItemId,
            WarehousePhysicalId = request.WarehousePhysicalId,
            BinId = request.BinId,
            BatchId = request.BatchId,
            DeltaValue = request.DeltaValue,
            Reason = reason!,
            Timestamp = timestamp,
            User = user
        };

        _db.ValueAdjustments.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<IReadOnlyList<ValueAdjustmentDto>> GetHistoryAsync(
        Guid itemId,
        Guid? warehousePhysicalId,
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
                x.BinId,
                x.BatchId,
                x.DeltaValue,
                x.Reason,
                x.Timestamp,
                x.User))
            .ToListAsync(ct);
    }

    private static ValueAdjustmentDto ToDto(ValueAdjustment entity) => new(
        entity.Id,
        entity.ItemId,
        entity.WarehousePhysicalId,
        entity.BinId,
        entity.BatchId,
        entity.DeltaValue,
        entity.Reason,
        entity.Timestamp,
        entity.User);
}
