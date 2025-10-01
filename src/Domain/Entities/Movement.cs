namespace Lkvitai.Warehouse.Domain.Entities;

public class Movement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocNo { get; set; } = string.Empty;
    public string Type { get; set; } = "IN"; // IN / ADJUST / MOVE
    public string Status { get; set; } = "POSTED";

    public Guid ItemId { get; set; }

    // Source (for IN/ADJUST can be null)
    public Guid? WarehousePhysicalId { get; set; }
    public Guid? BinId { get; set; }

    // Destination (for MOVE; null for IN/ADJUST)
    public Guid? ToWarehousePhysicalId { get; set; }
    public Guid? ToBinId { get; set; }

    public Guid? BatchId { get; set; } // reserved for later
    public string? BatchNo { get; set; }
    public DateTime? BatchMfgDate { get; set; }
    public DateTime? BatchExpDate { get; set; }
    public string? BatchQuality { get; set; }

    public decimal QtyBase { get; set; }  // for MOVE must be > 0
    public string Uom { get; set; } = "pcs";
    public decimal Factor { get; set; } = 1m;

    public string? Reason { get; set; }
    public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Ref { get; set; }
    public string? MetaJson { get; set; }
}
