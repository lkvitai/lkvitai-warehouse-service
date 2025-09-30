namespace Lkvitai.Warehouse.Domain.Entities;

public class InventoryCount
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid ItemId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? BatchId { get; set; }
    public decimal QtyCounted { get; set; }
    public decimal QtySystemAtStart { get; set; }
    public decimal Delta => QtyCounted - QtySystemAtStart;
    public DateTimeOffset CountedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CountedBy { get; set; }
}
