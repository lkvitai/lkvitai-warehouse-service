using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Services;

public class ValueAdjustmentService
{
    private readonly WarehouseDbContext _db;
    public ValueAdjustmentService(WarehouseDbContext db) => _db = db;

    public async Task<ValueAdjustment> CreateAsync(ValueAdjustment adjustment, CancellationToken ct = default)
    {
        if (adjustment.ItemId == Guid.Empty)
            throw new InvalidOperationException("ItemId is required for value adjustment.");
        if (adjustment.DeltaValue == 0)
            throw new InvalidOperationException("DeltaValue cannot be zero.");

        if (adjustment.Timestamp == default)
            adjustment.Timestamp = DateTimeOffset.UtcNow;

        _db.ValueAdjustments.Add(adjustment);
        await _db.SaveChangesAsync(ct);
        return adjustment;
    }

    public IQueryable<ValueAdjustment> Query() => _db.ValueAdjustments.AsNoTracking();
}
