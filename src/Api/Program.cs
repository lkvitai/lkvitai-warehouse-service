using Lkvitai.Warehouse.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DB/Infra
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers + Swagger UI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lkvitai Warehouse", Version = "v1" });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Dev-seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Lkvitai.Warehouse.Infrastructure.Persistence.WarehouseDbContext>();
    await Lkvitai.Warehouse.Infrastructure.Seed.DevSeed.EnsureAsync(db);
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lkvitai Warehouse v1");
});

app.MapHealthChecks("/health");
app.MapControllers();
app.MapGet("/", () => "Warehouse API up");

await app.RunAsync();
