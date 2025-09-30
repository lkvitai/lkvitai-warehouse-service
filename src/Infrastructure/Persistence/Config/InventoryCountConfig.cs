using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class InventoryCountConfig : IEntityTypeConfiguration<InventoryCount>
{
    public void Configure(EntityTypeBuilder<InventoryCount> b)
    {
        b.ToTable("inventory_count", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.SessionId);
        b.Property(x => x.ItemId);
        b.Property(x => x.BinId);
        b.Property(x => x.BatchId);
        b.Property(x => x.QtyCounted).HasColumnType("numeric(18,6)");
        b.Property(x => x.QtySystemAtStart).HasColumnType("numeric(18,6)");
        b.Property(x => x.CountedAt);
    }
}
