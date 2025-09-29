namespace Lkvitai.Warehouse.Application.Dto;
public record WarehousePhysicalDto(Guid Id, string Code, string Name, string? Address, Guid? LogicalId, bool IsActive);
