using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class BinConfig : IEntityTypeConfiguration<Bin>
{
    public void Configure(EntityTypeBuilder<Bin> b)
    {
        b.ToTable("bin", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.WarehousePhysicalId).HasColumnType("uuid");
        b.Property(x => x.WarehouseZoneId).HasColumnType("uuid");
        b.Property(x => x.WarehouseRackId).HasColumnType("uuid");
        b.Property(x => x.Code).IsRequired().HasMaxLength(64);
        b.HasIndex(x => new { x.WarehousePhysicalId, x.Code }).IsUnique();
        b.Property(x => x.Kind).IsRequired().HasMaxLength(32);
        b.Property(x => x.MetaJson).HasColumnType("jsonb");

        b.HasOne<WarehousePhysical>()
            .WithMany()
            .HasForeignKey(x => x.WarehousePhysicalId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<WarehouseZone>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne<WarehouseRack>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseRackId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
