using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lkvitai.Warehouse.Infrastructure.Persistence
{
    public class WarehouseDesignTimeFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
    {
        public WarehouseDbContext CreateDbContext(string[] args)
        {
            var cs = Environment.GetEnvironmentVariable("WH_CS")
                     ?? "Host=10.8.0.11;Port=5432;Database=lkvitai-mes-wh;Username=app_user;Password=app_pass";

            var options = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseNpgsql(cs)
                .Options;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            return new WarehouseDbContext(options);
        }
    }
}