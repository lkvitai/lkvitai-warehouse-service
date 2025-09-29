using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/warehouse-physicals")]
public class WarehousePhysicalsController : ControllerBase
{
    private readonly WarehouseDbContext _db;
    public WarehousePhysicalsController(WarehouseDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<WarehousePhysicalDto>> GetAll() =>
        await _db.WarehousePhysicals.AsNoTracking()
          .Select(x => new WarehousePhysicalDto(x.Id,x.Code,x.Name,x.Address,x.LogicalId,x.IsActive)).ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehousePhysicalDto>> Get(Guid id)
    {
        var e = await _db.WarehousePhysicals.FindAsync(id);
        if (e == null) return NotFound();
        return new WarehousePhysicalDto(e.Id,e.Code,e.Name,e.Address,e.LogicalId,e.IsActive);
    }

    [HttpPost]
    public async Task<ActionResult<WarehousePhysicalDto>> Create(WarehousePhysicalDto dto)
    {
        var e = new WarehousePhysical { Code=dto.Code, Name=dto.Name, Address=dto.Address, LogicalId=dto.LogicalId, IsActive=dto.IsActive };
        _db.WarehousePhysicals.Add(e); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.Id }, new WarehousePhysicalDto(e.Id,e.Code,e.Name,e.Address,e.LogicalId,e.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, WarehousePhysicalDto dto)
    {
        var e = await _db.WarehousePhysicals.FindAsync(id);
        if (e == null) return NotFound();
        e.Code=dto.Code; e.Name=dto.Name; e.Address=dto.Address; e.LogicalId=dto.LogicalId; e.IsActive=dto.IsActive;
        await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var e = await _db.WarehousePhysicals.FindAsync(id);
        if (e == null) return NotFound();
        _db.WarehousePhysicals.Remove(e); await _db.SaveChangesAsync(); return NoContent();
    }
}
