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
        // Проверки по-минимуму
        if (m.QtyBase == 0) throw new InvalidOperationException("QtyBase cannot be zero.");
        if (m.Type is not ("IN" or "ADJUST")) throw new InvalidOperationException("Only IN/ADJUST supported in MVP.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // 1) Запись движения
        _db.Movements.Add(m);
        await _db.SaveChangesAsync(ct);

        // 2) Обновление остатков (upsert)
        var bal = await _db.StockBalances
            .SingleOrDefaultAsync(b =>
                b.ItemId == m.ItemId &&
                b.WarehousePhysicalId == m.WarehousePhysicalId &&
                b.BinId == m.BinId &&
                b.BatchId == m.BatchId, ct);

        if (bal is null)
        {
            bal = new StockBalance
            {
                ItemId = m.ItemId,
                WarehousePhysicalId = m.WarehousePhysicalId,
                BinId = m.BinId,
                BatchId = m.BatchId,
                QtyBase = 0m
            };
            _db.StockBalances.Add(bal);
        }

        bal.QtyBase += m.QtyBase; // для IN положительное, для ADJUST +/- как пришло
        bal.UpdatedAt = DateTimeOffset.UtcNow;

        // Нельзя уйти в минус (MVP-правило)
        if (bal.QtyBase < 0)
            throw new InvalidOperationException("Resulting balance would be negative.");

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return m;
    }
}