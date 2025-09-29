namespace Lkvitai.Warehouse.Application.Dto;

public record ItemDto(Guid Id, string Sku, string Name, string UomBase, bool IsActive);
