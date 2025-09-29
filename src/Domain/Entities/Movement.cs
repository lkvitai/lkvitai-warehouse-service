namespace Lkvitai.Warehouse.Domain.Entities;

public class Movement
{
    public Guid Id { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public string Type { get; set; } = "IN"; // IN / ADJUST (пока)
    public string Status { get; set; } = "POSTED"; // DRAFT/POSTED (пока POSTED)
    public Guid ItemId { get; set; }
    public Guid? WarehousePhysicalId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? BatchId { get; set; } // зарезервировано
    public decimal QtyBase { get; set; }  // + для IN, +/- для ADJUST
    public string Uom { get; set; } = "pcs";
    public decimal Factor { get; set; } = 1m;
    public string? Reason { get; set; }
    public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Ref { get; set; }
    public string? MetaJson { get; set; }
}