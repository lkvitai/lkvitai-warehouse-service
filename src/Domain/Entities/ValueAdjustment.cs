namespace Lkvitai.Warehouse.Domain.Entities;

public class ValueAdjustment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public Guid? WarehousePhysicalId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? BatchId { get; set; }
    public decimal DeltaValue { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? PerformedBy { get; set; }
}
