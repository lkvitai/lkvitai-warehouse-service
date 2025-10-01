using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Application.Exports;
using Lkvitai.Warehouse.Application.Exports.Contracts;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/exports")]
public sealed class ExportsController : ControllerBase
{
    private readonly IAgnumExportService _agnumExportService;
    private readonly WarehouseDbContext _db;

    public ExportsController(IAgnumExportService agnumExportService, WarehouseDbContext db)
    {
        _agnumExportService = agnumExportService;
        _db = db;
    }

    [HttpPost("agnum/run")]
    public async Task<ActionResult<ExportJobDto>> RunAgnum([FromBody] AgnumExportRequest request, CancellationToken ct)
    {
        var job = await _agnumExportService.RunAsync(request, ct);
        return ExportJobDto.FromEntity(job);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExportJobDto>> Get(Guid id, CancellationToken ct)
    {
        var job = await _db.ExportJobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (job is null)
        {
            return NotFound();
        }

        return ExportJobDto.FromEntity(job);
    }
}
