using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Lkvitai.Warehouse.Infrastructure;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("Warehouse")!;
        services.AddDbContext<WarehouseDbContext>(o => o.UseNpgsql(cs));
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        return services;
    }
}