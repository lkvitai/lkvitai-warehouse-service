namespace Lkvitai.Warehouse.Domain.Entities;

public sealed class ValueAdjustment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public Guid? WarehousePhysicalId { get; set; }
    public Guid? WarehouseLogicalId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? BatchId { get; set; }
    public decimal DeltaValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? UserId { get; set; }

    public Item? Item { get; set; }
    public WarehousePhysical? WarehousePhysical { get; set; }
    public WarehouseLogical? WarehouseLogical { get; set; }
    public Bin? Bin { get; set; }
    public StockBatch? Batch { get; set; }
}
