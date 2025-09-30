namespace Lkvitai.Warehouse.Domain.Entities;

public class InventorySession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public Guid WarehousePhysicalId { get; set; }
    public string Status { get; set; } = "OPEN"; // OPEN/CLOSED/POSTED
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
}
