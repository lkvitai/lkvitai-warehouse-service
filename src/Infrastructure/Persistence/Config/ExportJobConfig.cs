using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public sealed class ExportJobConfig : IEntityTypeConfiguration<ExportJob>
{
    public void Configure(EntityTypeBuilder<ExportJob> b)
    {
        b.ToTable("export_job", "job");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        b.Property(x => x.SliceType)
            .HasMaxLength(32)
            .IsRequired();

        b.Property(x => x.SliceKey)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.Format)
            .HasMaxLength(16)
            .IsRequired();

        b.Property(x => x.Status)
            .HasMaxLength(32)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(x => x.StartedAt)
            .HasColumnType("timestamptz");

        b.Property(x => x.CompletedAt)
            .HasColumnType("timestamptz");

        b.Property(x => x.ExportedAtUtc)
            .HasColumnType("timestamptz");

        b.Property(x => x.FilePath)
            .HasMaxLength(512);

        b.Property(x => x.ErrorFilePath)
            .HasMaxLength(512);

        b.Property(x => x.ErrorMessage)
            .HasMaxLength(1024);

        b.HasIndex(x => x.Status);
        b.HasIndex(x => new { x.SliceType, x.SliceKey });
    }
}
