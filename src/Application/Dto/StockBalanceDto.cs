namespace Lkvitai.Warehouse.Application.Dto;

public record StockBalanceDto(
    Guid Id,
    Guid ItemId,
    Guid? WarehousePhysicalId,
    Guid? BinId,
    Guid? BatchId,
    decimal QtyBase,
    DateTimeOffset UpdatedAt
);
