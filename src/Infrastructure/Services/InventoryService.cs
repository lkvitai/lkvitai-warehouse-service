using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Infrastructure.Services;

public class InventoryService
{
    private readonly WarehouseDbContext _db;
    private readonly MovementService _movements;
    public InventoryService(WarehouseDbContext db, MovementService movements)
    {
        _db = db; _movements = movements;
    }

    public async Task<InventorySession> OpenAsync(Guid warehouseId, string code, CancellationToken ct = default)
    {
        var s = new InventorySession { WarehousePhysicalId = warehouseId, Code = code, Status = "OPEN" };
        _db.InventorySessions.Add(s);
        await _db.SaveChangesAsync(ct);
        return s;
    }

    public async Task<InventoryCount> AddCountAsync(Guid sessionId, Guid itemId, Guid? binId, decimal counted, string? user, CancellationToken ct = default)
    {
        var s = await _db.InventorySessions.FindAsync(new object?[] { sessionId }, ct);
        if (s is null || s.Status != "OPEN") throw new InvalidOperationException("Session not open.");

        var sysQty = await _db.StockBalances
            .Where(b => b.ItemId == itemId && b.BinId == binId)
            .SumAsync(b => (decimal?)b.QtyBase, ct) ?? 0m;

        var c = new InventoryCount
        {
            SessionId = sessionId,
            ItemId = itemId,
            BinId = binId,
            QtyCounted = counted,
            QtySystemAtStart = sysQty,
            CountedBy = user
        };
        _db.InventoryCounts.Add(c);
        await _db.SaveChangesAsync(ct);
        return c;
    }

    public async Task<List<Movement>> PostAsync(Guid sessionId, CancellationToken ct = default)
    {
        var s = await _db.InventorySessions.FindAsync(new object?[] { sessionId }, ct);
        if (s is null) throw new InvalidOperationException("Session not found.");
        if (s.Status != "OPEN") return new List<Movement>(); // idempotent

        var counts = await _db.InventoryCounts.Where(c => c.SessionId == sessionId).ToListAsync(ct);

        var moves = new List<Movement>();
        foreach (var c in counts)
        {
            if (c.Delta != 0)
            {
                var m = new Movement
                {
                    Type = "ADJUST",
                    ItemId = c.ItemId,
                    WarehousePhysicalId = s.WarehousePhysicalId,
                    BinId = c.BinId,
                    QtyBase = c.Delta,
                    Reason = "inventory"
                };
                await _movements.PostAsync(m, ct);
                moves.Add(m);
            }
        }

        s.Status = "POSTED";
        await _db.SaveChangesAsync(ct);
        return moves;
    }

    public async Task<InventorySession?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.InventorySessions.FindAsync(new object?[] { id }, ct);
    }
}
