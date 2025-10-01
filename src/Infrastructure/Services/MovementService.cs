using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Services;

public class MovementService
{
    private readonly WarehouseDbContext _db;
    public MovementService(WarehouseDbContext db) => _db = db;

    public async Task<Movement> PostAsync(Movement m, CancellationToken ct = default)
    {
        if (m.QtyBase == 0) throw new InvalidOperationException("QtyBase cannot be zero.");
        m.Type = m.Type.ToUpperInvariant();

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        switch (m.Type)
        {
            case "IN":
                await EnsureBatchLifecycleAsync(m, ct, allowCreate: true);
                await PostInOrAdjustAsync(m, ct);
                break;

            case "ADJUST":
                await EnsureBatchLifecycleAsync(m, ct, allowCreate: false);
                await PostInOrAdjustAsync(m, ct);
                break;

            case "MOVE":
                await EnsureBatchLifecycleAsync(m, ct, allowCreate: false);

                if (m.QtyBase <= 0) throw new InvalidOperationException("MOVE requires QtyBase > 0.");
                if (m.BinId is null || m.WarehousePhysicalId is null)
                    throw new InvalidOperationException("MOVE requires source WarehousePhysicalId and BinId.");
                if (m.ToBinId is null || m.ToWarehousePhysicalId is null)
                    throw new InvalidOperationException("MOVE requires destination ToWarehousePhysicalId and ToBinId.");

                _db.Movements.Add(m);
                await _db.SaveChangesAsync(ct);

                var src = await FindOrCreateBalanceAsync(
                    m.ItemId, m.WarehousePhysicalId, m.BinId, m.BatchId, ct);

                if (src.QtyBase < m.QtyBase)
                    throw new InvalidOperationException("Insufficient stock at source for MOVE.");

                src.QtyBase -= m.QtyBase;
                src.UpdatedAt = DateTimeOffset.UtcNow;

                var dst = await FindOrCreateBalanceAsync(
                    m.ItemId, m.ToWarehousePhysicalId, m.ToBinId, m.BatchId, ct);

                dst.QtyBase += m.QtyBase;
                dst.UpdatedAt = DateTimeOffset.UtcNow;

                await _db.SaveChangesAsync(ct);
                break;

            default:
                throw new InvalidOperationException("Supported types: IN/ADJUST/MOVE.");
        }

        await tx.CommitAsync(ct);
        return m;
    }

    private async Task EnsureBatchLifecycleAsync(Movement m, CancellationToken ct, bool allowCreate)
    {
        if (m.BatchId is null)
        {
            if (!string.IsNullOrWhiteSpace(m.BatchNo) || m.BatchMfgDate.HasValue || m.BatchExpDate.HasValue || !string.IsNullOrWhiteSpace(m.BatchQuality))
                throw new InvalidOperationException("Batch metadata provided without BatchId.");
            return;
        }

        var existing = await _db.StockBatches.FindAsync(new object?[] { m.BatchId.Value }, ct);
        if (existing is null)
        {
            if (!allowCreate)
                throw new InvalidOperationException("Batch does not exist for this movement.");

            var newBatch = new StockBatch
            {
                Id = m.BatchId.Value,
                ItemId = m.ItemId,
                BatchNo = string.IsNullOrWhiteSpace(m.BatchNo)
                    ? m.BatchId.Value.ToString("N")[..8]
                    : m.BatchNo,
                MfgDate = m.BatchMfgDate,
                ExpDate = m.BatchExpDate,
                Quality = ResolveBatchQuality(m.BatchQuality),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.StockBatches.Add(newBatch);
            await _db.SaveChangesAsync(ct);
            return;
        }

        if (existing.ItemId != m.ItemId)
            throw new InvalidOperationException("Batch belongs to another item.");

        var touched = false;

        if (!string.IsNullOrWhiteSpace(m.BatchNo) && !string.Equals(existing.BatchNo, m.BatchNo, StringComparison.Ordinal))
        {
            existing.BatchNo = m.BatchNo;
            touched = true;
        }
        if (m.BatchMfgDate.HasValue && existing.MfgDate != m.BatchMfgDate)
        {
            existing.MfgDate = m.BatchMfgDate;
            touched = true;
        }
        if (m.BatchExpDate.HasValue && existing.ExpDate != m.BatchExpDate)
        {
            existing.ExpDate = m.BatchExpDate;
            touched = true;
        }
        if (!string.IsNullOrWhiteSpace(m.BatchQuality))
        {
            var newQuality = ResolveBatchQuality(m.BatchQuality);
            if (existing.Quality != newQuality)
            {
                existing.Quality = newQuality;
                touched = true;
            }
        }

        if (touched)
        {
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task PostInOrAdjustAsync(Movement m, CancellationToken ct)
    {
        _db.Movements.Add(m);
        await _db.SaveChangesAsync(ct);

        var bal = await FindOrCreateBalanceAsync(m.ItemId, m.WarehousePhysicalId, m.BinId, m.BatchId, ct);

        bal.QtyBase += m.QtyBase;
        if (bal.QtyBase < 0)
            throw new InvalidOperationException("Resulting balance would be negative.");

        bal.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<StockBalance> FindOrCreateBalanceAsync(Guid itemId, Guid? whId, Guid? binId, Guid? batchId, CancellationToken ct)
    {
        var bal = await _db.StockBalances.SingleOrDefaultAsync(b =>
            b.ItemId == itemId &&
            b.WarehousePhysicalId == whId &&
            b.BinId == binId &&
            b.BatchId == batchId, ct);

        if (bal is null)
        {
            bal = new StockBalance
            {
                ItemId = itemId,
                WarehousePhysicalId = whId,
                BinId = binId,
                BatchId = batchId,
                QtyBase = 0m,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.StockBalances.Add(bal);
            await _db.SaveChangesAsync(ct);
        }
        return bal;
    }

    private static BatchQuality ResolveBatchQuality(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return BatchQuality.Ok;

        return Enum.TryParse<BatchQuality>(value, true, out var parsed) ? parsed : BatchQuality.Ok;
    }
}
