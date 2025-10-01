using Lkvitai.Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lkvitai.Warehouse.Infrastructure.Persistence.Config;

public sealed class ExportScheduleConfig : IEntityTypeConfiguration<ExportSchedule>
{
    public void Configure(EntityTypeBuilder<ExportSchedule> b)
    {
        b.ToTable("export_schedule", "job");
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

        b.Property(x => x.Enabled)
            .HasColumnType("boolean")
            .HasDefaultValue(true);

        b.Property(x => x.Cron)
            .HasMaxLength(128);

        b.Property(x => x.LastRunAt)
            .HasColumnType("timestamptz");

        b.Property(x => x.NextRunAt)
            .HasColumnType("timestamptz");

        b.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.HasIndex(x => x.Enabled);
        b.HasIndex(x => new { x.SliceType, x.SliceKey }).IsUnique();
    }
}
