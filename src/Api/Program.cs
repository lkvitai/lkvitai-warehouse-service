using Lkvitai.Warehouse.Application.Exports;
using Lkvitai.Warehouse.Infrastructure;
using Lkvitai.Warehouse.Infrastructure.Options;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Lkvitai.Warehouse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

if (env.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<WarehouseDbContext>(o =>
        o.UseInMemoryDatabase("it_api_" + Guid.NewGuid()));
    builder.Services.Configure<AgnumExportOptions>(builder.Configuration.GetSection("Exports:Agnum"));
    builder.Services.AddScoped<IAgnumExportService, AgnumExportService>();
}
else
{
    builder.Services.AddInfrastructure(builder.Configuration);
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lkvitai Warehouse", Version = "v1" });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!env.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    await Lkvitai.Warehouse.Infrastructure.Seed.DevSeed.EnsureAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lkvitai Warehouse v1"));

app.MapHealthChecks("/health");
app.MapControllers();
app.MapGet("/", () => "Warehouse API up");

await app.RunAsync();

public partial class Program { }
