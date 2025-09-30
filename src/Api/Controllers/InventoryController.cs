using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _svc;
    public InventoryController(InventoryService svc) => _svc = svc;

    [HttpPost("sessions")]
    public async Task<ActionResult<InventorySession>> Open(Guid warehouseId, string code)
        => await _svc.OpenAsync(warehouseId, code);

    [HttpPost("{id}/counts")]
    public async Task<ActionResult<InventoryCount>> AddCount(Guid id, Guid itemId, Guid? binId, decimal counted, string? user = null)
        => await _svc.AddCountAsync(id, itemId, binId, counted, user);

    [HttpPost("{id}/post")]
    public async Task<ActionResult<List<Movement>>> Post(Guid id)
        => await _svc.PostAsync(id);

    [HttpGet("{id}")]
    public async Task<ActionResult<InventorySession>> Get(Guid id)
    {
        var session = await _svc.GetAsync(id);
        if (session is null) return NotFound();
        return Ok(session);
    }
}
