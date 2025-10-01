using System.Globalization;
using Lkvitai.Warehouse.Application.Exports;
using Lkvitai.Warehouse.Application.Exports.Contracts;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Options;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lkvitai.Warehouse.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<Worker> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);
    private readonly AgnumExportOptions _options;

    public Worker(IServiceProvider services, ILogger<Worker> logger, IOptions<AgnumExportOptions> options)
    {
        _services = services;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agnum export worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agnum export worker iteration failed");
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var exportService = scope.ServiceProvider.GetRequiredService<IAgnumExportService>();

        var schedules = await db.ExportSchedules
            .Where(x => x.Enabled)
            .ToListAsync(ct);

        if (schedules.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var runTime = ParseDailyRun(_options.DailyRunAtUtc);

        foreach (var schedule in schedules)
        {
            var nextRun = schedule.NextRunAt ?? ComputeNextRun(now, runTime);

            if (nextRun > now)
            {
                schedule.NextRunAt = nextRun;
                continue;
            }

            var request = new AgnumExportRequest(schedule.SliceType, schedule.SliceKey);

            _logger.LogInformation("Executing scheduled Agnum export {SliceType}:{SliceKey}", schedule.SliceType, schedule.SliceKey);

            var job = await exportService.RunAsync(request, ct);

            schedule.LastRunAt = job.CompletedAt ?? now;
            schedule.NextRunAt = ComputeNextRun(now, runTime);
        }

        await db.SaveChangesAsync(ct);
    }

    private static DateTimeOffset ComputeNextRun(DateTimeOffset referenceUtc, TimeSpan runAtUtc)
    {
        var todayRun = new DateTimeOffset(referenceUtc.Year, referenceUtc.Month, referenceUtc.Day,
            runAtUtc.Hours, runAtUtc.Minutes, runAtUtc.Seconds, TimeSpan.Zero);

        if (todayRun <= referenceUtc)
        {
            todayRun = todayRun.AddDays(1);
        }

        return todayRun;
    }

    private static TimeSpan ParseDailyRun(string dailyRunAtUtc)
    {
        if (TimeSpan.TryParse(dailyRunAtUtc, CultureInfo.InvariantCulture, out var time))
        {
            return time;
        }

        return new TimeSpan(19, 30, 0);
    }
}
