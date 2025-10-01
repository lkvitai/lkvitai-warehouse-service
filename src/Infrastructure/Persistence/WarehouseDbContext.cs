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

    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();

    public DbSet<InventorySession> InventorySessions => Set<InventorySession>();
    public DbSet<InventoryCount> InventoryCounts => Set<InventoryCount>();
    public DbSet<ValueAdjustment> ValueAdjustments => Set<ValueAdjustment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("pgcrypto");
        b.HasDefaultSchema("wh");
        b.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}