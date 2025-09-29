namespace Lkvitai.Warehouse.Domain.Entities;
public class Item
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string UomBase { get; set; } = "pcs";
    public bool IsActive { get; set; } = true;
    public string? AttrsJson { get; set; } // jsonb
}