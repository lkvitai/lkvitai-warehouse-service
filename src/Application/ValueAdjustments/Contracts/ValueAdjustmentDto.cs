namespace Lkvitai.Warehouse.Application.ValueAdjustments.Contracts;

public sealed record ValueAdjustmentDto(
    Guid Id,
    Guid ItemId,
    Guid WarehousePhysicalId,
    Guid? BinId,
    Guid? BatchId,
    decimal DeltaValue,
    string Reason,
    DateTimeOffset Timestamp,
    string User
);
