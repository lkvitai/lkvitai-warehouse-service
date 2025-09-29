# Lkvitai.Warehouse Service

Minimal MVP: .NET, EF Core (PostgreSQL), CRUD Items, EF migrations.

## Dev run
```bash
dotnet run --project src/Api/Lkvitai.Warehouse.Api.csproj --urls "http://127.0.0.1:5000"
EF Migrations (Design-time factory)
```
Open: http://127.0.0.1:5000/swagger

## EF Migrations (Design-time factory)
```bash
$env:WH_CS = "Host=10.8.0.11;Port=5432;Database=lkvitai-mes-wh;Username=app_user;Password=app_pass"
dotnet ef migrations add <Name> -p src/Infrastructure -o Persistence/Migrations
dotnet ef database update -p src/Infrastructure
```