using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class MovementConfig : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> b)
    {
        b.ToTable("movement", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.DocNo).HasMaxLength(64);
        b.Property(x => x.Type).IsRequired().HasMaxLength(16);
        b.Property(x => x.Status).IsRequired().HasMaxLength(16);

        b.Property(x => x.ItemId).HasColumnType("uuid");
        b.Property(x => x.WarehousePhysicalId).HasColumnType("uuid");
        b.Property(x => x.BinId).HasColumnType("uuid");

        b.Property(x => x.ToWarehousePhysicalId).HasColumnType("uuid");
        b.Property(x => x.ToBinId).HasColumnType("uuid");

        b.Property(x => x.BatchId).HasColumnType("uuid");
        b.Property(x => x.QtyBase).HasColumnType("numeric(18,6)");
        b.Property(x => x.Uom).HasMaxLength(16);
        b.Property(x => x.Factor).HasColumnType("numeric(18,6)");
        b.Property(x => x.Reason).HasMaxLength(256);
        b.Property(x => x.PerformedAt).HasColumnType("timestamptz");
        b.Property(x => x.MetaJson).HasColumnType("jsonb");

        b.HasIndex(x => x.PerformedAt);
        b.HasIndex(x => new { x.ItemId, x.BinId, x.BatchId });
        b.HasIndex(x => new { x.ItemId, x.ToBinId, x.BatchId });
    }
}
