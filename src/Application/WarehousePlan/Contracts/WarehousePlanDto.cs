namespace Lkvitai.Warehouse.Application.WarehousePlan.Contracts;

public record WarehousePlanDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<WarehousePlanZoneDto> Zones);

public record WarehousePlanZoneDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<WarehousePlanRackDto> Racks);

public record WarehousePlanRackDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<WarehousePlanBinDto> Bins);

public record WarehousePlanBinDto(
    Guid Id,
    string Code,
    decimal QtyBase);

public record LocateBinDto(
    Guid BinId,
    string BinCode,
    string WarehouseCode,
    string? ZoneCode,
    string? RackCode,
    decimal QtyBase);
