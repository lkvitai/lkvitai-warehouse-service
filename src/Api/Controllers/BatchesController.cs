using Lkvitai.Warehouse.Application.Batches;
using Lkvitai.Warehouse.Application.Batches.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/batches")]
public class BatchesController : ControllerBase
{
    private readonly IBatchQueryService _service;
    public BatchesController(IBatchQueryService service) => _service = service;

    [HttpGet("{batchId:guid}")]
    public async Task<ActionResult<BatchDetailsDto>> Get(Guid batchId, CancellationToken ct)
    {
        var dto = await _service.GetDetailsAsync(batchId, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    [HttpGet("{batchId:guid}/trace")]
    public async Task<ActionResult<BatchTraceDto>> GetTrace(Guid batchId, CancellationToken ct)
    {
        var dto = await _service.GetTraceAsync(batchId, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }
}
