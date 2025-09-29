using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly WarehouseDbContext _db;
    public ItemsController(WarehouseDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAll()
    {
        return await _db.Items
            .AsNoTracking()
            .Select(i => new ItemDto(i.Id, i.Sku, i.Name, i.UomBase, i.IsActive))
            .ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> Get(Guid id)
    {
        var i = await _db.Items.FindAsync(id);
        if (i == null) return NotFound();
        return new ItemDto(i.Id, i.Sku, i.Name, i.UomBase, i.IsActive);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(ItemDto dto)
    {
        var entity = new Item { Sku = dto.Sku, Name = dto.Name, UomBase = dto.UomBase, IsActive = dto.IsActive };
        _db.Items.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new ItemDto(entity.Id, entity.Sku, entity.Name, entity.UomBase, entity.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ItemDto dto)
    {
        var entity = await _db.Items.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Sku = dto.Sku; entity.Name = dto.Name; entity.UomBase = dto.UomBase; entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Items.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Items.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
