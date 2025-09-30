using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class ItemConfig : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> b)
    {
        b.ToTable("item", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.Sku).IsRequired().HasMaxLength(64);
        b.HasIndex(x => x.Sku).IsUnique();
        b.Property(x => x.Name).IsRequired().HasMaxLength(256);
        b.Property(x => x.UomBase).IsRequired().HasMaxLength(16);
        b.Property(x => x.AttrsJson).HasColumnType("jsonb");
    }
}
