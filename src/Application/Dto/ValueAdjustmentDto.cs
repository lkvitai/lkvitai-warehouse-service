namespace Lkvitai.Warehouse.Application.Dto;

public record ValueAdjustmentDto(
    Guid Id,
    Guid ItemId,
    Guid? WarehousePhysicalId,
    Guid? BinId,
    Guid? BatchId,
    decimal DeltaValue,
    string? Reason,
    DateTimeOffset Timestamp,
    string? PerformedBy
);
