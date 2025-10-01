namespace Lkvitai.Warehouse.Application.Dto;

public sealed record ExportJobDto(
    Guid Id,
    string SliceType,
    string SliceKey,
    string Format,
    string Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? FilePath,
    string? ErrorFilePath,
    string? ErrorMessage)
{
    public static ExportJobDto FromEntity(Lkvitai.Warehouse.Domain.Entities.ExportJob job) =>
        new(job.Id, job.SliceType, job.SliceKey, job.Format, job.Status, job.StartedAt, job.CompletedAt, job.FilePath, job.ErrorFilePath, job.ErrorMessage);
}
