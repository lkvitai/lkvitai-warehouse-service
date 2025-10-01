using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class WarehouseZoneConfig : IEntityTypeConfiguration<WarehouseZone>
{
    public void Configure(EntityTypeBuilder<WarehouseZone> b)
    {
        b.ToTable("warehouse_zone", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.WarehousePhysicalId).HasColumnType("uuid");
        b.Property(x => x.Code).IsRequired().HasMaxLength(64);
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.Property(x => x.MetaJson).HasColumnType("jsonb");
        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.HasIndex(x => new { x.WarehousePhysicalId, x.Code }).IsUnique();

        b.HasOne<WarehousePhysical>()
            .WithMany()
            .HasForeignKey(x => x.WarehousePhysicalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
