namespace Lkvitai.Warehouse.Domain.Entities;

public sealed class ValueAdjustment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public Guid WarehousePhysicalId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? BatchId { get; set; }
    public decimal DeltaValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string User { get; set; } = string.Empty;

    public Item Item { get; set; } = null!;
    public WarehousePhysical WarehousePhysical { get; set; } = null!;
    public Bin? Bin { get; set; }
}
