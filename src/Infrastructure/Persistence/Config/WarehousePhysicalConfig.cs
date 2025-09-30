using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class WarehousePhysicalConfig : IEntityTypeConfiguration<WarehousePhysical>
{
    public void Configure(EntityTypeBuilder<WarehousePhysical> b)
    {
        b.ToTable("warehouse_physical", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.Code).IsRequired().HasMaxLength(64);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.Property(x => x.Address).HasMaxLength(256);
        b.Property(x => x.LogicalId).HasColumnType("uuid");
        b.Property(x => x.MetaJson).HasColumnType("jsonb");

        b.HasOne<WarehouseLogical>()
         .WithMany()
         .HasForeignKey(x => x.LogicalId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
