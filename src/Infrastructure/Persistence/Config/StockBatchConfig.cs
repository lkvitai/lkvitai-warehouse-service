using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public class StockBatchConfig : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> b)
    {
        b.ToTable("stock_batch", "wh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();
        b.Property(x => x.ItemId).HasColumnType("uuid");
        b.Property(x => x.BatchNo).IsRequired().HasMaxLength(128);
        b.Property(x => x.MfgDate).HasColumnType("date");
        b.Property(x => x.ExpDate).HasColumnType("date");
        b.Property(x => x.Quality)
            .HasConversion<int>()
            .HasColumnType("integer");
        b.Property(x => x.MetaJson).HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        b.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

        b.HasIndex(x => new { x.ItemId, x.BatchNo }).IsUnique();
    }
}
