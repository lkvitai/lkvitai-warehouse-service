namespace Lkvitai.Warehouse.Domain.Entities;

public enum BatchQuality
{
    Ok = 0,
    Quarantine = 1,
    Scrap = 2
}

public class StockBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? MfgDate { get; set; }
    public DateTime? ExpDate { get; set; }
    public BatchQuality Quality { get; set; } = BatchQuality.Ok;
    public string? MetaJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<StockBalance> Balances { get; set; } = new List<StockBalance>();
    public ICollection<BatchLineage> Parents { get; set; } = new List<BatchLineage>();
    public ICollection<BatchLineage> Children { get; set; } = new List<BatchLineage>();
}
