using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class StockBalanceConfig : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> b)
    {
        b.ToTable("stock_balance", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.ItemId).HasColumnType("uuid");
        b.Property(x => x.WarehousePhysicalId).HasColumnType("uuid");
        b.Property(x => x.BinId).HasColumnType("uuid");
        b.Property(x => x.BatchId).HasColumnType("uuid");
        b.Property(x => x.QtyBase).HasColumnType("numeric(18,6)");
        b.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

        b.HasOne(x => x.Batch)
            .WithMany(x => x.Balances)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.ItemId, x.WarehousePhysicalId, x.BinId, x.BatchId }).IsUnique();
    }
}
