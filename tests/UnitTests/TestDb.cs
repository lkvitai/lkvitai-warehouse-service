using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Lkvitai.Warehouse.UnitTests;

public sealed class TestDb : IAsyncDisposable
{
    private readonly SqliteConnection _conn;
    public WarehouseDbContext Db { get; }

    public TestDb()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open(); // важно, иначе БД исчезнет

        var opts = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_conn)
            .Options;

        Db = new WarehouseDbContext(opts);
        Db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _conn.DisposeAsync();
    }
}
