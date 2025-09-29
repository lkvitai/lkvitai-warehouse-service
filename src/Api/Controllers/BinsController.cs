using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/bins")]
public class BinsController : ControllerBase
{
    private readonly WarehouseDbContext _db;
    public BinsController(WarehouseDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<BinDto>> GetAll([FromQuery] Guid? warehousePhysicalId = null)
    {
        var q = _db.Bins.AsNoTracking().AsQueryable();
        if (warehousePhysicalId is not null) q = q.Where(b => b.WarehousePhysicalId == warehousePhysicalId);
        return await q.Select(b => new BinDto(b.Id,b.WarehousePhysicalId,b.Code,b.Kind,b.IsActive)).ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BinDto>> Get(Guid id)
    {
        var e = await _db.Bins.FindAsync(id);
        if (e == null) return NotFound();
        return new BinDto(e.Id,e.WarehousePhysicalId,e.Code,e.Kind,e.IsActive);
    }

    [HttpPost]
    public async Task<ActionResult<BinDto>> Create(BinDto dto)
    {
        var e = new Bin { WarehousePhysicalId=dto.WarehousePhysicalId, Code=dto.Code, Kind=dto.Kind, IsActive=dto.IsActive };
        _db.Bins.Add(e); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.Id }, new BinDto(e.Id,e.WarehousePhysicalId,e.Code,e.Kind,e.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, BinDto dto)
    {
        var e = await _db.Bins.FindAsync(id);
        if (e == null) return NotFound();
        e.WarehousePhysicalId=dto.WarehousePhysicalId; e.Code=dto.Code; e.Kind=dto.Kind; e.IsActive=dto.IsActive;
        await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var e = await _db.Bins.FindAsync(id);
        if (e == null) return NotFound();
        _db.Bins.Remove(e); await _db.SaveChangesAsync(); return NoContent();
    }
}
