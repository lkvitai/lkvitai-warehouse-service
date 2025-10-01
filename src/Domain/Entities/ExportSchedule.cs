namespace Lkvitai.Warehouse.Domain.Entities;

public sealed class ExportSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SliceType { get; set; } = string.Empty;
    public string SliceKey { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public TimeOnly? AtUtc { get; set; }
    public string? Cron { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public DateTimeOffset? NextRunAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
