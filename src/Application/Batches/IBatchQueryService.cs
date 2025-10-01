using Lkvitai.Warehouse.Application.Batches.Contracts;

namespace Lkvitai.Warehouse.Application.Batches;

public interface IBatchQueryService
{
    Task<BatchDetailsDto?> GetDetailsAsync(Guid batchId, CancellationToken ct = default);
    Task<BatchTraceDto?> GetTraceAsync(Guid batchId, CancellationToken ct = default);
}
