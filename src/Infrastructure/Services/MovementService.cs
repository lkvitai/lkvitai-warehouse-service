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
            case "ADJUST":
                await PostInOrAdjustAsync(m, ct);
                break;

            case "MOVE":
                if (m.QtyBase <= 0) throw new InvalidOperationException("MOVE requires QtyBase > 0.");
                if (m.BinId is null || m.WarehousePhysicalId is null)
                    throw new InvalidOperationException("MOVE requires source WarehousePhysicalId and BinId.");
                if (m.ToBinId is null || m.ToWarehousePhysicalId is null)
                    throw new InvalidOperationException("MOVE requires destination ToWarehousePhysicalId and ToBinId.");

                // 1) write movement
                _db.Movements.Add(m);
                await _db.SaveChangesAsync(ct);

                // 2) debit source
                var src = await FindOrCreateBalanceAsync(
                    m.ItemId, m.WarehousePhysicalId, m.BinId, m.BatchId, ct);

                if (src.QtyBase < m.QtyBase)
                    throw new InvalidOperationException("Insufficient stock at source for MOVE.");

                src.QtyBase -= m.QtyBase;
                src.UpdatedAt = DateTimeOffset.UtcNow;

                // 3) credit dest
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

    private async Task PostInOrAdjustAsync(Movement m, CancellationToken ct)
    {
        _db.Movements.Add(m);
        await _db.SaveChangesAsync(ct);

        var bal = await FindOrCreateBalanceAsync(m.ItemId, m.WarehousePhysicalId, m.BinId, m.BatchId, ct);

        bal.QtyBase += m.QtyBase; // for ADJUST allow +/- as provided
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
                QtyBase = 0m
            };
            _db.StockBalances.Add(bal);
            await _db.SaveChangesAsync(ct);
        }
        return bal;
    }
}
