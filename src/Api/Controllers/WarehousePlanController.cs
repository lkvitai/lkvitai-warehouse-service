using Lkvitai.Warehouse.Application.WarehousePlan;
using Lkvitai.Warehouse.Application.WarehousePlan.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/plan")]
public class WarehousePlanController : ControllerBase
{
    private readonly IWarehousePlanService _planService;
    public WarehousePlanController(IWarehousePlanService planService) => _planService = planService;

    [HttpGet("{warehouseId:guid}")]
    public async Task<ActionResult<WarehousePlanDto>> GetPlan(Guid warehouseId, CancellationToken ct)
    {
        var plan = await _planService.GetPlanAsync(warehouseId, ct);
        if (plan is null) return NotFound();
        return Ok(plan);
    }

    [HttpGet("{warehouseId:guid}/locate")]
    public async Task<ActionResult<IEnumerable<LocateBinDto>>> Locate(Guid warehouseId, [FromQuery] Guid itemId, [FromQuery] Guid? batchId, CancellationToken ct)
    {
        try
        {
            var locations = await _planService.LocateAsync(warehouseId, itemId, batchId, ct);
            return Ok(locations);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
