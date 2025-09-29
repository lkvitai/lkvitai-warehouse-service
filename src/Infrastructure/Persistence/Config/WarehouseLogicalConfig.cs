using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class WarehouseLogicalConfig : IEntityTypeConfiguration<WarehouseLogical>
{
    public void Configure(EntityTypeBuilder<WarehouseLogical> b)
    {
        b.ToTable("warehouse_logical", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Code).IsRequired().HasMaxLength(64);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.Property(x => x.Kind).IsRequired().HasMaxLength(32);
        b.Property(x => x.Tags).HasMaxLength(256);
        b.Property(x => x.MetaJson).HasColumnType("jsonb");
        b.Property(x => x.ParentId).HasColumnType("uuid");
    }
}