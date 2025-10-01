namespace Lkvitai.Warehouse.Domain.Entities;
public class Bin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WarehousePhysicalId { get; set; }
    public Guid? WarehouseZoneId { get; set; }
    public Guid? WarehouseRackId { get; set; }
    public string Code { get; set; } = null!;   // unique per warehouse
    public string Kind { get; set; } = "STORAGE"; // PICK/STORAGE/RECEIVE/SHIP
    public bool IsActive { get; set; } = true;
    public string? MetaJson { get; set; } // jsonb
}
