using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class ValueAdjustmentConfig : IEntityTypeConfiguration<ValueAdjustment>
{
    public void Configure(EntityTypeBuilder<ValueAdjustment> b)
    {
        b.ToTable("value_adjustment", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.ItemId).HasColumnType("uuid");
        b.Property(x => x.WarehousePhysicalId).HasColumnType("uuid");
        b.Property(x => x.BinId).HasColumnType("uuid");
        b.Property(x => x.BatchId).HasColumnType("uuid");
        b.Property(x => x.DeltaValue).HasColumnType("numeric(18,2)");
        b.Property(x => x.Reason).HasMaxLength(256);
        b.Property(x => x.Timestamp).HasColumnType("timestamptz");
        b.Property(x => x.PerformedBy).HasColumnName("performed_by").HasMaxLength(128);

        b.HasIndex(x => x.Timestamp);
        b.HasIndex(x => new { x.ItemId, x.BatchId, x.WarehousePhysicalId, x.BinId });
    }
}
