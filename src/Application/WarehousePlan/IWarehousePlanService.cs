using Lkvitai.Warehouse.Application.WarehousePlan.Contracts;

namespace Lkvitai.Warehouse.Application.WarehousePlan;

public interface IWarehousePlanService
{
    Task<WarehousePlanDto?> GetPlanAsync(Guid warehouseId, CancellationToken ct = default);
    Task<IEnumerable<LocateBinDto>> LocateAsync(Guid warehouseId, Guid itemId, Guid? batchId, CancellationToken ct = default);
}
