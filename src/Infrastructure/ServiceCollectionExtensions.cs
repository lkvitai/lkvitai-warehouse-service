using System;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lkvitai.Warehouse.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
        {
            // Берём строку подключения из appsettings; если пусто — пробуем из переменной окружения (на dev это WH_CS)
            var cs = cfg.GetConnectionString("Warehouse");
            if (string.IsNullOrWhiteSpace(cs))
                cs = Environment.GetEnvironmentVariable("WH_CS");

            if (!string.IsNullOrWhiteSpace(cs))
            {
                services.AddDbContext<WarehouseDbContext>(o => o.UseNpgsql(cs));
            }
            else
            {
                // Fallback для локальной разработки (можешь удалить, если не нужен)
                services.AddDbContext<WarehouseDbContext>(o =>
                    o.UseNpgsql("Host=localhost;Port=5432;Database=lkvitai-mes-wh;Username=app_user;Password=app_pass"));
            }

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // DI сервисов домена/инфры
            services.AddScoped<MovementService>();
            services.AddScoped<InventoryService>();

            return services;
        }
    }
}
