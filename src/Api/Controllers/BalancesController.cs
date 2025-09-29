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

    // Фильтры: itemId, warehousePhysicalId, binId
    [HttpGet]
    public async Task<IEnumerable<StockBalanceDto>> Get(
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? warehousePhysicalId = null,
        [FromQuery] Guid? binId = null)
    {
        var q = _db.StockBalances.AsNoTracking().AsQueryable();

        if (itemId is not null) q = q.Where(x => x.ItemId == itemId);
        if (warehousePhysicalId is not null) q = q.Where(x => x.WarehousePhysicalId == warehousePhysicalId);
        if (binId is not null) q = q.Where(x => x.BinId == binId);

        return await q
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new StockBalanceDto(x.Id, x.ItemId, x.WarehousePhysicalId, x.BinId, x.BatchId, x.QtyBase, x.UpdatedAt))
            .ToListAsync();
    }

    // Быстрый total по item/складу/ячейке
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
