namespace Lkvitai.Warehouse.Domain.Entities;

public class WarehouseRack
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WarehouseZoneId { get; set; }
    public string Code { get; set; } = string.Empty; // unique per zone
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? MetaJson { get; set; }
}
