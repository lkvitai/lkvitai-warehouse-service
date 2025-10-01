using Lkvitai.Warehouse.Application.Dto;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.Api.Controllers;

[ApiController]
[Route("api/movements")]
public class MovementsController : ControllerBase
{
    private readonly WarehouseDbContext _db;
    private readonly MovementService _svc;
    public MovementsController(WarehouseDbContext db, MovementService svc)
    {
        _db = db; _svc = svc;
    }

    [HttpGet]
    public async Task<IEnumerable<MovementDto>> List([FromQuery] int take = 100)
    {
        take = Math.Clamp(take, 1, 1000);
        return await _db.Movements.AsNoTracking()
            .OrderByDescending(x => x.PerformedAt)
            .Take(take)
            .Select(m => new MovementDto(
                m.Id,
                m.Type,
                m.ItemId,
                m.WarehousePhysicalId,
                m.BinId,
                m.QtyBase,
                m.Reason,
                m.BatchId,
                m.BatchNo))
            .ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MovementDto>> GetById(Guid id)
    {
        var m = await _db.Movements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return NotFound();
        return new MovementDto(
            m.Id,
            m.Type,
            m.ItemId,
            m.WarehousePhysicalId,
            m.BinId,
            m.QtyBase,
            m.Reason,
            m.BatchId,
            m.BatchNo);
    }

    public record PostMovementRequest(
        string Type,
        Guid ItemId,
        Guid? WarehousePhysicalId,
        Guid? BinId,
        decimal QtyBase,
        string? Reason,
        Guid? ToWarehousePhysicalId,
        Guid? ToBinId,
        Guid? BatchId,
        string? BatchNo,
        DateTime? BatchMfgDate,
        DateTime? BatchExpDate,
        string? BatchQuality
    );

    [HttpPost]
    public async Task<ActionResult<MovementDto>> Post(PostMovementRequest req)
    {
        var m = new Movement
        {
            Type = req.Type,
            ItemId = req.ItemId,
            WarehousePhysicalId = req.WarehousePhysicalId,
            BinId = req.BinId,
            QtyBase = req.QtyBase,
            Reason = req.Reason,
            ToWarehousePhysicalId = req.ToWarehousePhysicalId,
            ToBinId = req.ToBinId,
            BatchId = req.BatchId,
            BatchNo = req.BatchNo,
            BatchMfgDate = req.BatchMfgDate,
            BatchExpDate = req.BatchExpDate,
            BatchQuality = req.BatchQuality
        };

        var saved = await _svc.PostAsync(m);
        var dto = new MovementDto(
            saved.Id,
            saved.Type,
            saved.ItemId,
            saved.WarehousePhysicalId,
            saved.BinId,
            saved.QtyBase,
            saved.Reason,
            saved.BatchId,
            saved.BatchNo);
        return CreatedAtAction(nameof(GetById), new { id = saved.Id }, dto);
    }
}
