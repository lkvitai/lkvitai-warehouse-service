namespace Lkvitai.Warehouse.Domain.Entities;

public class StockBalance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public Guid? WarehousePhysicalId { get; set; }
    public Guid? WarehouseLogicalId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? BatchId { get; set; }
    public decimal QtyBase { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public StockBatch? Batch { get; set; }
    public WarehouseLogical? WarehouseLogical { get; set; }
}
