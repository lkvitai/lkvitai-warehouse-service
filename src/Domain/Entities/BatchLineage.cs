namespace Lkvitai.Warehouse.Domain.Entities;

public enum BatchRelationType
{
    Split = 0,
    Merge = 1,
    Relabel = 2,
    Convert = 3
}

public class BatchLineage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentBatchId { get; set; }
    public Guid ChildBatchId { get; set; }
    public BatchRelationType RelationType { get; set; } = BatchRelationType.Split;
    public decimal QtyBase { get; set; }
    public Guid? MovementId { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public StockBatch? ParentBatch { get; set; }
    public StockBatch ChildBatch { get; set; } = null!;
    public Movement? Movement { get; set; }
}
