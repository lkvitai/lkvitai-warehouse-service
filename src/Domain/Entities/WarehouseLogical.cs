namespace Lkvitai.Warehouse.Domain.Entities;
public class WarehouseLogical
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Kind { get; set; } = "DEFAULT"; // DEFAULT/CUSTOM
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }
    public string? MetaJson { get; set; } // jsonb
}
