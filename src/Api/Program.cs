using Lkvitai.Warehouse.Infrastructure;
using Microsoft.OpenApi.Models;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// DB/Infra
if (env.IsEnvironment("Testing"))
{
    // для интеграционных тестов: InMemory
    builder.Services.AddDbContext<WarehouseDbContext>(o =>
        o.UseInMemoryDatabase("it_api_" + Guid.NewGuid()));
}
else
{
    // обычный путь: Npgsql и остальная инфраструктура
    builder.Services.AddInfrastructure(builder.Configuration);
}

// Controllers + Swagger UI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lkvitai Warehouse", Version = "v1" });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Dev-seed — пропускаем в тестах
if (!env.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    await Lkvitai.Warehouse.Infrastructure.Seed.DevSeed.EnsureAsync(db);
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lkvitai Warehouse v1"));

app.MapHealthChecks("/health");
app.MapControllers();
app.MapGet("/", () => "Warehouse API up");

await app.RunAsync();

// для WebApplicationFactory
public partial class Program { }
