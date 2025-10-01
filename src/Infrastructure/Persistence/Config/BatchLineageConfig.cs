using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class BatchLineageConfig : IEntityTypeConfiguration<BatchLineage>
{
    public void Configure(EntityTypeBuilder<BatchLineage> b)
    {
        b.ToTable("batch_lineage", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.ParentBatchId).HasColumnType("uuid");
        b.Property(x => x.ChildBatchId).HasColumnType("uuid");
        b.Property(x => x.RelationType)
            .HasConversion<int>()
            .HasColumnType("integer");
        b.Property(x => x.QtyBase).HasColumnType("numeric(18,6)");
        b.Property(x => x.MovementId).HasColumnType("uuid");
        b.Property(x => x.Timestamp).HasColumnType("timestamptz");

        b.HasIndex(x => x.ChildBatchId);
        b.HasOne(x => x.ChildBatch)
            .WithMany(x => x.Parents)
            .HasForeignKey(x => x.ChildBatchId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ParentBatch)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentBatchId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Movement)
            .WithMany()
            .HasForeignKey(x => x.MovementId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
