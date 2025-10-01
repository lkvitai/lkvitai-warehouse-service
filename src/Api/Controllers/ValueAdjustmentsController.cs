using Lkvitai.Warehouse.Application.ValueAdjustments;
using Lkvitai.Warehouse.Application.ValueAdjustments.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/value-adjustments")]
public class ValueAdjustmentsController : ControllerBase
{
    private readonly IValueAdjustmentService _service;

    public ValueAdjustmentsController(IValueAdjustmentService service) => _service = service;

    /// <summary>Создать корректировку ценности (не влияет на количество)</summary>
    [HttpPost]
    public async Task<ActionResult<ValueAdjustmentDto>> Create(
        [FromBody] CreateValueAdjustmentRequest request,
        CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { itemId = created.ItemId }, created);
    }

    /// <summary>История корректировок по товару/партии/ячейке</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ValueAdjustmentDto>>> Get(
        [FromQuery] Guid itemId,
        [FromQuery] Guid? warehousePhysicalId,
        [FromQuery] Guid? binId,
        [FromQuery] Guid? batchId,
        CancellationToken ct)
    {
        if (itemId == Guid.Empty)
            return BadRequest("itemId is required.");

        var list = await _service.GetHistoryAsync(itemId, warehousePhysicalId, binId, batchId, ct);
        return Ok(list);
    }
}
