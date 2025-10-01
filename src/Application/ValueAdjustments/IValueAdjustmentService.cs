using Lkvitai.Warehouse.Application.ValueAdjustments.Contracts;

namespace Lkvitai.Warehouse.Application.ValueAdjustments;

public interface IValueAdjustmentService
{
    Task<ValueAdjustmentDto> CreateAsync(CreateValueAdjustmentRequest request, CancellationToken ct);
    Task<IReadOnlyList<ValueAdjustmentDto>> GetHistoryAsync(Guid itemId, Guid? warehousePhysicalId, Guid? binId, Guid? batchId, CancellationToken ct);
}
