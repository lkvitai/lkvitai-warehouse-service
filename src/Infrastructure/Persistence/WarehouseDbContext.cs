using Microsoft.EntityFrameworkCore;
using Lkvitai.Warehouse.Domain.Entities;

namespace Lkvitai.Warehouse.Infrastructure.Persistence;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<WarehouseLogical> WarehouseLogicals => Set<WarehouseLogical>();
    public DbSet<WarehousePhysical> WarehousePhysicals => Set<WarehousePhysical>();
    public DbSet<Bin> Bins => Set<Bin>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Для gen_random_uuid()
        b.HasPostgresExtension("pgcrypto");
        b.HasDefaultSchema("wh");
        b.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}