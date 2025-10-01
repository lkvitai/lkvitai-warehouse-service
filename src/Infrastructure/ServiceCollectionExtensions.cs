using System;
using Lkvitai.Warehouse.Application.Batches;
using Lkvitai.Warehouse.Application.Exports;
using Lkvitai.Warehouse.Application.ValueAdjustments;
using Lkvitai.Warehouse.Application.WarehousePlan;
using Lkvitai.Warehouse.Infrastructure.Options;
using Lkvitai.Warehouse.Infrastructure.Export;
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
            var cs = cfg.GetConnectionString("Warehouse");
            if (string.IsNullOrWhiteSpace(cs))
                cs = Environment.GetEnvironmentVariable("WH_CS");

            if (!string.IsNullOrWhiteSpace(cs))
            {
                services.AddDbContext<WarehouseDbContext>(o => o.UseNpgsql(cs));
            }
            else
            {
                services.AddDbContext<WarehouseDbContext>(o =>
                    o.UseNpgsql("Host=localhost;Port=5432;Database=lkvitai-mes-wh;Username=app_user;Password=app_pass"));
            }

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.Configure<AgnumExportOptions>(cfg.GetSection("Exports:Agnum"));
            services.AddSingleton<SftpUploader>();
            services.AddScoped<IAgnumExportService, AgnumExportService>();

            services.AddScoped<MovementService>();
            services.AddScoped<InventoryService>();
            services.AddScoped<IValueAdjustmentService, ValueAdjustmentService>();
            services.AddScoped<IWarehousePlanService, WarehousePlanService>();
            services.AddScoped<IBatchQueryService, BatchQueryService>();

            return services;
        }
    }
}
