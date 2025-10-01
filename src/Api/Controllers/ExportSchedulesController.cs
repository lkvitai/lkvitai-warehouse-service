using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/export-schedules")]
public class ExportSchedulesController : ControllerBase
{
    private readonly WarehouseDbContext _db;
    public ExportSchedulesController(WarehouseDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await _db.ExportSchedules.AsNoTracking().OrderBy(x => x.SliceType).ThenBy(x => x.SliceKey).ToListAsync(ct));

    public record UpsertDto(string SliceType, string SliceKey, bool Enabled, TimeOnly? AtUtc);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertDto dto, CancellationToken ct)
    {
        var m = new ExportSchedule { SliceType = dto.SliceType, SliceKey = dto.SliceKey, Enabled = dto.Enabled, AtUtc = dto.AtUtc };
        _db.ExportSchedules.Add(m);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = m.Id }, m);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var m = await _db.ExportSchedules.FindAsync(new object?[] { id }, ct);
        return m is null ? NotFound() : Ok(m);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertDto dto, CancellationToken ct)
    {
        var m = await _db.ExportSchedules.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();
        m.SliceType = dto.SliceType; m.SliceKey = dto.SliceKey; m.Enabled = dto.Enabled; m.AtUtc = dto.AtUtc;
        await _db.SaveChangesAsync(ct);
        return Ok(m);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var m = await _db.ExportSchedules.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();
        _db.ExportSchedules.Remove(m);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
