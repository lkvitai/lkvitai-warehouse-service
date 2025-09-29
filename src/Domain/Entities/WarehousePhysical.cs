namespace Lkvitai.Warehouse.Domain.Entities;
public class WarehousePhysical
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public Guid? LogicalId { get; set; }   // FK -> WarehouseLogical
    public bool IsActive { get; set; } = true;
    public string? MetaJson { get; set; }  // jsonb
}