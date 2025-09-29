namespace Lkvitai.Warehouse.Application.Dto;
public record BinDto(Guid Id, Guid WarehousePhysicalId, string Code, string Kind, bool IsActive);
