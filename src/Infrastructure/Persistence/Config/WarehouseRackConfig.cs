using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class WarehouseRackConfig : IEntityTypeConfiguration<WarehouseRack>
{
    public void Configure(EntityTypeBuilder<WarehouseRack> b)
    {
        b.ToTable("warehouse_rack", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.WarehouseZoneId).HasColumnType("uuid");
        b.Property(x => x.Code).IsRequired().HasMaxLength(64);
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.Property(x => x.MetaJson).HasColumnType("jsonb");
        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.HasIndex(x => new { x.WarehouseZoneId, x.Code }).IsUnique();

        b.HasOne<WarehouseZone>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
