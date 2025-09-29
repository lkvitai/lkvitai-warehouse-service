using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/warehouse-logicals")]
public class WarehouseLogicalsController : ControllerBase
{
    private readonly WarehouseDbContext _db;
    public WarehouseLogicalsController(WarehouseDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<WarehouseLogicalDto>> GetAll() =>
        await _db.WarehouseLogicals.AsNoTracking()
          .Select(x => new WarehouseLogicalDto(x.Id,x.Code,x.Name,x.Kind,x.IsActive)).ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseLogicalDto>> Get(Guid id)
    {
        var e = await _db.WarehouseLogicals.FindAsync(id);
        if (e == null) return NotFound();
        return new WarehouseLogicalDto(e.Id,e.Code,e.Name,e.Kind,e.IsActive);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseLogicalDto>> Create(WarehouseLogicalDto dto)
    {
        var e = new WarehouseLogical { Code=dto.Code, Name=dto.Name, Kind=dto.Kind, IsActive=dto.IsActive };
        _db.WarehouseLogicals.Add(e); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.Id }, new WarehouseLogicalDto(e.Id,e.Code,e.Name,e.Kind,e.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, WarehouseLogicalDto dto)
    {
        var e = await _db.WarehouseLogicals.FindAsync(id);
        if (e == null) return NotFound();
        e.Code=dto.Code; e.Name=dto.Name; e.Kind=dto.Kind; e.IsActive=dto.IsActive;
        await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var e = await _db.WarehouseLogicals.FindAsync(id);
        if (e == null) return NotFound();
        _db.WarehouseLogicals.Remove(e); await _db.SaveChangesAsync(); return NoContent();
    }
}
