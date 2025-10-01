namespace Lkvitai.Warehouse.Domain.Entities;

public sealed class ExportJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SliceType { get; set; } = string.Empty;
    public string SliceKey { get; set; } = string.Empty;
    public string Format { get; set; } = ExportJobFormat.Csv;
    public string Status { get; set; } = ExportJobStatus.Queued;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorFilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? ExportedAtUtc { get; set; }
}

public static class ExportJobStatus
{
    public const string Queued = "Queued";
    public const string Running = "Running";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
}

public static class ExportJobFormat
{
    public const string Csv = "CSV";
}
