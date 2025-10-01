using System.Linq;
using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/balances")]
public class BalancesController : ControllerBase
{
    private readonly WarehouseDbContext _db;
    public BalancesController(WarehouseDbContext db) => _db = db;

    // Filters: itemId, warehousePhysicalId, binId
    [HttpGet]
    public async Task<IEnumerable<StockBalanceDto>> Get(
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? warehousePhysicalId = null,
        [FromQuery] Guid? binId = null)
    {
        var balances = _db.StockBalances.AsNoTracking().AsQueryable();

        if (itemId is not null) balances = balances.Where(x => x.ItemId == itemId);
        if (warehousePhysicalId is not null) balances = balances.Where(x => x.WarehousePhysicalId == warehousePhysicalId);
        if (binId is not null) balances = balances.Where(x => x.BinId == binId);

        var rows = await balances
            .Select(b => new StockBalanceDto(
                b.Id,
                b.ItemId,
                b.WarehousePhysicalId,
                b.BinId,
                b.BatchId,
                b.QtyBase,
                b.UpdatedAt,
                (from adj in _db.ValueAdjustments.AsNoTracking()
                 where adj.ItemId == b.ItemId
                       && adj.WarehousePhysicalId == b.WarehousePhysicalId
                       && adj.BinId == b.BinId
                       && adj.BatchId == b.BatchId
                 select (decimal?)adj.DeltaValue).Sum() ?? 0m))
            .ToListAsync();

        return rows.OrderByDescending(x => x.UpdatedAt);
    }

    // Quick total by item/warehouse/bin
    [HttpGet("total")]
    public async Task<decimal> GetTotal(
        [FromQuery] Guid itemId,
        [FromQuery] Guid? warehousePhysicalId = null,
        [FromQuery] Guid? binId = null)
    {
        var q = _db.StockBalances.AsNoTracking().Where(x => x.ItemId == itemId);
        if (warehousePhysicalId is not null) q = q.Where(x => x.WarehousePhysicalId == warehousePhysicalId);
        if (binId is not null) q = q.Where(x => x.BinId == binId);
        return await q.SumAsync(x => x.QtyBase);
    }
}
