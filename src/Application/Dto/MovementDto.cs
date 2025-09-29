namespace Lkvitai.Warehouse.Application.Dto;

public record MovementDto(
    Guid Id,
    string Type,
    Guid ItemId,
    Guid? WarehousePhysicalId,
    Guid? BinId,
    decimal QtyBase,
    string? Reason
);