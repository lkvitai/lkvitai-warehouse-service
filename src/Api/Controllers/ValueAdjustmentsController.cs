using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/value-adjustments")]
public class ValueAdjustmentsController : ControllerBase
{
    private readonly ValueAdjustmentService _service;

    public ValueAdjustmentsController(ValueAdjustmentService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<ValueAdjustmentDto>> Post(
        [FromBody] CreateValueAdjustmentRequest request,
        CancellationToken ct)
    {
        var entity = new ValueAdjustment
        {
            ItemId = request.ItemId,
            WarehousePhysicalId = request.WarehousePhysicalId,
            BinId = request.BinId,
            BatchId = request.BatchId,
            DeltaValue = request.DeltaValue,
            Reason = request.Reason,
            Timestamp = request.Timestamp ?? DateTimeOffset.UtcNow,
            PerformedBy = request.PerformedBy
        };

        var saved = await _service.CreateAsync(entity, ct);
        return Ok(ToDto(saved));
    }

    [HttpGet]
    public async Task<IReadOnlyList<ValueAdjustmentDto>> Get(
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? warehousePhysicalId = null,
        [FromQuery] Guid? binId = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int take = 200,
        CancellationToken ct = default)
    {
        if (take <= 0) take = 1;
        if (take > 500) take = 500;

        var query = _service.Query();

        if (itemId.HasValue && itemId.Value != Guid.Empty)
            query = query.Where(x => x.ItemId == itemId.Value);
        if (warehousePhysicalId.HasValue)
            query = query.Where(x => x.WarehousePhysicalId == warehousePhysicalId.Value);
        if (binId.HasValue)
            query = query.Where(x => x.BinId == binId.Value);
        if (batchId.HasValue)
            query = query.Where(x => x.BatchId == batchId.Value);
        if (from.HasValue)
            query = query.Where(x => x.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.Timestamp <= to.Value);

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Take(take)
            .Select(x => new ValueAdjustmentDto(
                x.Id,
                x.ItemId,
                x.WarehousePhysicalId,
                x.BinId,
                x.BatchId,
                x.DeltaValue,
                x.Reason,
                x.Timestamp,
                x.PerformedBy))
            .ToListAsync(ct);
    }

    private static ValueAdjustmentDto ToDto(ValueAdjustment entity) => new(
        entity.Id,
        entity.ItemId,
        entity.WarehousePhysicalId,
        entity.BinId,
        entity.BatchId,
        entity.DeltaValue,
        entity.Reason,
        entity.Timestamp,
        entity.PerformedBy);
}

public record CreateValueAdjustmentRequest(
    Guid ItemId,
    Guid? WarehousePhysicalId,
    Guid? BinId,
    Guid? BatchId,
    decimal DeltaValue,
    string? Reason,
    DateTimeOffset? Timestamp,
    string? PerformedBy);
