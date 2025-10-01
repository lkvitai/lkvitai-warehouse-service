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

        b.Property(x => x.ItemId)
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(x => x.WarehousePhysicalId)
            .HasColumnType("uuid");

        b.Property(x => x.WarehouseLogicalId)
            .HasColumnType("uuid");

        b.Property(x => x.BinId)
            .HasColumnType("uuid");

        b.Property(x => x.BatchId)
            .HasColumnType("uuid");

        b.Property(x => x.DeltaValue)
            .HasColumnType("numeric(18,6)")
            .IsRequired();

        b.Property(x => x.Reason)
            .HasMaxLength(512)
            .IsRequired();

        b.Property(x => x.Timestamp)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(128);

        b.HasIndex(x => x.ItemId);
        b.HasIndex(x => x.WarehouseLogicalId);
        b.HasIndex(x => new { x.ItemId, x.WarehousePhysicalId, x.WarehouseLogicalId, x.BinId, x.BatchId });

        b.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.WarehousePhysical)
            .WithMany()
            .HasForeignKey(x => x.WarehousePhysicalId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.WarehouseLogical)
            .WithMany()
            .HasForeignKey(x => x.WarehouseLogicalId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Bin)
            .WithMany()
            .HasForeignKey(x => x.BinId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
