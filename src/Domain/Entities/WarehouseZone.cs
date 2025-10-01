namespace Lkvitai.Warehouse.Domain.Entities;

public class WarehouseZone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WarehousePhysicalId { get; set; }
    public string Code { get; set; } = string.Empty; // unique per warehouse
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? MetaJson { get; set; }
}
