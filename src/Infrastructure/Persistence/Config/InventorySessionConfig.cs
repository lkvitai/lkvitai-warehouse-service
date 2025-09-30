using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class InventorySessionConfig : IEntityTypeConfiguration<InventorySession>
{
    public void Configure(EntityTypeBuilder<InventorySession> b)
    {
        b.ToTable("inventory_session", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Code).HasMaxLength(64);
        b.Property(x => x.WarehousePhysicalId);
        b.Property(x => x.Status).HasMaxLength(16);
        b.Property(x => x.CreatedAt);
    }
}
