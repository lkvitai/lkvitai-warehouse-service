namespace Lkvitai.Warehouse.Application.Batches.Contracts;

public record BatchDetailsDto(
    Guid Id,
    Guid ItemId,
    string Sku,
    string Name,
    string BatchNo,
    DateTime? MfgDate,
    DateTime? ExpDate,
    string? Quality,
    IReadOnlyList<BatchLocationDto> Locations);

public record BatchLocationDto(
    Guid? WarehouseId,
    string WarehouseCode,
    Guid? BinId,
    string? BinCode,
    string? ZoneCode,
    string? RackCode,
    decimal QtyBase);

public record BatchTraceDto(
    Guid BatchId,
    string Sku,
    IReadOnlyList<BatchTraceMovementDto> Movements,
    IReadOnlyList<BatchTraceLocationDto> Locations);

public record BatchTraceMovementDto(
    Guid Id,
    string Type,
    decimal QtyBase,
    DateTimeOffset PerformedAt,
    Guid? WarehouseId,
    Guid? BinId,
    Guid? ToWarehouseId,
    Guid? ToBinId);

public record BatchTraceLocationDto(
    string Path,
    decimal QtyBase);
