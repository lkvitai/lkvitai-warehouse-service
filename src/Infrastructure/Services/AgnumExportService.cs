using System.Globalization;
using System.Text;
using Lkvitai.Warehouse.Application.Exports;
using Lkvitai.Warehouse.Application.Exports.Contracts;
using Lkvitai.Warehouse.Domain.Entities;
using Lkvitai.Warehouse.Infrastructure.Options;
using Lkvitai.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lkvitai.Warehouse.Infrastructure.Services;

public sealed class AgnumExportService : IAgnumExportService
{
    private readonly WarehouseDbContext _db;
    private readonly AgnumExportOptions _options;
    private readonly ILogger<AgnumExportService> _logger;

    public AgnumExportService(
        WarehouseDbContext db,
        IOptions<AgnumExportOptions> options,
        ILogger<AgnumExportService> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExportJob> RunAsync(AgnumExportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var utcNow = DateTimeOffset.UtcNow;
        var job = new ExportJob
        {
            SliceType = request.SliceType,
            SliceKey = request.SliceKey,
            Format = request.Format,
            Status = ExportJobStatus.Queued,
            CreatedAt = utcNow
        };

        _db.ExportJobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        job.Status = ExportJobStatus.Running;
        job.StartedAt = utcNow;
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await GenerateAsync(request, utcNow, cancellationToken);

            job.Status = ExportJobStatus.Succeeded;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.FilePath = result.FilePath;
            job.ErrorFilePath = result.ErrorFilePath;
            job.ExportedAtUtc = utcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agnum export {JobId} failed", job.Id);

            job.Status = ExportJobStatus.Failed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = ex.Message;

            await _db.SaveChangesAsync(cancellationToken);
            return job;
        }
    }

    private void ValidateRequest(AgnumExportRequest request)
    {
        if (!string.Equals(request.Format, ExportJobFormat.Csv, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only CSV export supported", nameof(request));

        if (string.IsNullOrWhiteSpace(request.SliceType))
            throw new ArgumentException("SliceType required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.SliceKey))
            throw new ArgumentException("SliceKey required", nameof(request));

        if (!IsSupportedSlice(request.SliceType))
            throw new ArgumentException($"SliceType '{request.SliceType}' not supported", nameof(request));
    }

    private static bool IsSupportedSlice(string sliceType) =>
        string.Equals(sliceType, AgnumExportRequest.Types.Physical, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(sliceType, AgnumExportRequest.Types.Total, StringComparison.OrdinalIgnoreCase);

    private async Task<GenerationResult> GenerateAsync(AgnumExportRequest request, DateTimeOffset exportAtUtc, CancellationToken ct)
    {
        EnsureDirectories();

        var sanitizedSliceKey = SanitizeForFileName(request.SliceKey);
        var timestamp = exportAtUtc.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var fileName = $"stock_{request.SliceType}_{sanitizedSliceKey}_{timestamp}.csv";
        var filePath = Path.Combine(_options.OutDir, fileName);
        var tempPath = Path.Combine(_options.OutDir, $".{Guid.NewGuid():N}_{fileName}");

        var errorFileName = Path.GetFileNameWithoutExtension(fileName) + "_errors.csv";
        var errorFilePath = Path.Combine(_options.ErrorsDir, errorFileName);
        var tempErrorPath = Path.Combine(_options.ErrorsDir, $".{Guid.NewGuid():N}_{errorFileName}");

        var data = await LoadDataAsync(request, ct);
        IReadOnlyList<AgnumRow> rows = request.SliceType.Equals(AgnumExportRequest.Types.Physical, StringComparison.OrdinalIgnoreCase)
            ? BuildPhysicalRows(data, exportAtUtc)
            : BuildTotalRows(data, request.SliceKey, exportAtUtc);

        var (validRows, errorRows) = ValidateRows(rows);

        if (validRows.Count == 0 && errorRows.Count > 0)
            throw new InvalidOperationException("Export contains only invalid rows; see error file");

        await WriteCsvAsync(tempPath, validRows, ct);
        File.Move(tempPath, filePath, overwrite: true);

        string? writtenErrorFile = null;
        if (errorRows.Count > 0)
        {
            await WriteErrorCsvAsync(tempErrorPath, errorRows, ct);
            File.Move(tempErrorPath, errorFilePath, overwrite: true);
            writtenErrorFile = errorFilePath;
        }

        if (_options.CopyToArchive)
        {
            var archiveFile = Path.Combine(_options.ArchiveDir, fileName);
            Directory.CreateDirectory(_options.ArchiveDir);
            File.Copy(filePath, archiveFile, overwrite: true);
        }

        return new GenerationResult(filePath, writtenErrorFile);
    }

    private async Task<IReadOnlyList<BalanceRecord>> LoadDataAsync(AgnumExportRequest request, CancellationToken ct)
    {
        var physicalFilter = request.SliceType.Equals(AgnumExportRequest.Types.Physical, StringComparison.OrdinalIgnoreCase);

        var baseQuery = from balance in _db.StockBalances.AsNoTracking()
                        join item in _db.Items.AsNoTracking() on balance.ItemId equals item.Id
                        join warehouse in _db.WarehousePhysicals.AsNoTracking() on balance.WarehousePhysicalId equals warehouse.Id into physJoin
                        from warehouse in physJoin.DefaultIfEmpty()
                        join bin in _db.Bins.AsNoTracking() on balance.BinId equals bin.Id into binJoin
                        from bin in binJoin.DefaultIfEmpty()
                        join batch in _db.StockBatches.AsNoTracking() on balance.BatchId equals batch.Id into batchJoin
                        from batch in batchJoin.DefaultIfEmpty()
                        select new
                        {
                            balance,
                            item,
                            warehouse,
                            bin,
                            batch
                        };

        if (!string.Equals(request.SliceKey, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            baseQuery = baseQuery.Where(x => x.warehouse != null && x.warehouse.Code == request.SliceKey);
        }

        if (physicalFilter)
        {
            baseQuery = baseQuery.Where(x => x.balance.BinId != null && x.bin != null);
        }

        var data = await baseQuery.ToListAsync(ct);
        if (data.Count == 0)
        {
            return Array.Empty<BalanceRecord>();
        }

        var keySet = data.Select(x => new AdjustmentKey(x.balance.ItemId, x.balance.WarehousePhysicalId, x.balance.BinId, x.balance.BatchId)).ToHashSet();
        var adjustments = await LoadAdjustmentsAsync(keySet, ct);

        return data.Select(x =>
        {
            var key = new AdjustmentKey(x.balance.ItemId, x.balance.WarehousePhysicalId, x.balance.BinId, x.balance.BatchId);
            adjustments.TryGetValue(key, out var adjValue);

            return new BalanceRecord(
                x.balance.ItemId,
                x.balance.WarehousePhysicalId,
                x.warehouse?.Code,
                x.balance.BinId,
                x.bin?.Code,
                x.balance.BatchId,
                x.batch?.BatchNo,
                x.item.Sku,
                x.item.UomBase,
                x.balance.QtyBase,
                adjValue,
                x.batch?.Quality.ToString(),
                x.batch?.ExpDate);
        }).ToList();
    }

    private async Task<Dictionary<AdjustmentKey, decimal>> LoadAdjustmentsAsync(HashSet<AdjustmentKey> keySet, CancellationToken ct)
    {
        if (keySet.Count == 0)
        {
            return new();
        }

        var itemIds = keySet.Select(k => k.ItemId).Distinct().ToArray();
        var warehouseIds = keySet.Select(k => k.WarehouseId).Where(v => v.HasValue).Select(v => v!.Value).Distinct().ToArray();

        var query = _db.ValueAdjustments.AsNoTracking().Where(x => itemIds.Contains(x.ItemId));
        if (warehouseIds.Length > 0)
        {
            query = query.Where(x => warehouseIds.Contains(x.WarehousePhysicalId));
        }

        var adjustments = await query.ToListAsync(ct);

        return adjustments
            .Select(adj => new { Key = new AdjustmentKey(adj.ItemId, adj.WarehousePhysicalId, adj.BinId, adj.BatchId), adj.DeltaValue })
            .Where(x => keySet.Contains(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.DeltaValue));
    }

    private static IReadOnlyList<AgnumRow> BuildPhysicalRows(IReadOnlyList<BalanceRecord> records, DateTimeOffset exportAtUtc)
    {
        if (records.Count == 0)
        {
            return Array.Empty<AgnumRow>();
        }

        return records
            .GroupBy(r => new { r.ItemId, r.ItemCode, r.BaseUoM, r.WarehouseCode, r.BinCode, r.BatchCode, r.Quality, r.ExpDate })
            .Select(g => new AgnumRow(
                exportAtUtc,
                AgnumExportRequest.Types.Physical,
                BuildPhysicalSliceKey(g.Key.WarehouseCode, g.Key.BinCode),
                g.Key.ItemCode,
                g.Key.BaseUoM,
                g.Sum(x => x.QtyBase),
                g.Sum(x => x.AdjValue),
                g.Key.BatchCode,
                g.Key.BinCode,
                g.Key.Quality,
                g.Key.ExpDate))
            .ToList();
    }

    private static IReadOnlyList<AgnumRow> BuildTotalRows(IReadOnlyList<BalanceRecord> records, string sliceKey, DateTimeOffset exportAtUtc)
    {
        if (records.Count == 0)
        {
            return Array.Empty<AgnumRow>();
        }

        return records
            .GroupBy(r => new { r.ItemId, r.ItemCode, r.BaseUoM })
            .Select(g => new AgnumRow(
                exportAtUtc,
                AgnumExportRequest.Types.Total,
                sliceKey,
                g.Key.ItemCode,
                g.Key.BaseUoM,
                g.Sum(x => x.QtyBase),
                g.Sum(x => x.AdjValue),
                BatchCode: null,
                LocationCode: string.Empty,
                Quality: null,
                ExpDate: null))
            .ToList();
    }

    private static (List<AgnumRow> ValidRows, List<ErrorRow> Errors) ValidateRows(IReadOnlyList<AgnumRow> rows)
    {
        var valid = new List<AgnumRow>(rows.Count);
        var errors = new List<ErrorRow>();

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.ItemCode))
            {
                errors.Add(new ErrorRow(row.ExportAtUtc, row.SliceType, row.SliceKey, row.ItemCode, "Missing ItemCode"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.BaseUoM))
            {
                errors.Add(new ErrorRow(row.ExportAtUtc, row.SliceType, row.SliceKey, row.ItemCode, "Missing BaseUoM"));
                continue;
            }

            if (row.SliceType == AgnumExportRequest.Types.Physical && string.IsNullOrWhiteSpace(row.LocationCode))
            {
                errors.Add(new ErrorRow(row.ExportAtUtc, row.SliceType, row.SliceKey, row.ItemCode, "Missing LocationCode"));
                continue;
            }

            valid.Add(row);
        }

        return (valid, errors);
    }

    private static async Task WriteCsvAsync(string filePath, IReadOnlyList<AgnumRow> rows, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await writer.WriteLineAsync("ExportAtUtc,SliceType,SliceKey,ItemCode,BaseUoM,QtyBase,AdjValue,BatchCode,LocationCode,Quality,ExpDate");

        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            var line = string.Join(',',
                Escape(row.ExportAtUtc.ToString("o", CultureInfo.InvariantCulture)),
                Escape(row.SliceType),
                Escape(row.SliceKey),
                Escape(row.ItemCode),
                Escape(row.BaseUoM),
                Escape(row.QtyBase.ToString("0.####", CultureInfo.InvariantCulture)),
                Escape(row.AdjValue.ToString("0.00", CultureInfo.InvariantCulture)),
                Escape(row.BatchCode),
                Escape(row.LocationCode),
                Escape(row.Quality),
                Escape(row.ExpDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

            await writer.WriteLineAsync(line);
        }
    }

    private static async Task WriteErrorCsvAsync(string filePath, IReadOnlyList<ErrorRow> errors, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await writer.WriteLineAsync("ExportAtUtc,SliceType,SliceKey,ItemCode,Reason");

        foreach (var error in errors)
        {
            ct.ThrowIfCancellationRequested();
            var line = string.Join(',',
                Escape(error.ExportAtUtc.ToString("o", CultureInfo.InvariantCulture)),
                Escape(error.SliceType),
                Escape(error.SliceKey),
                Escape(error.ItemCode),
                Escape(error.Reason));
            await writer.WriteLineAsync(line);
        }
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(_options.OutDir);
        Directory.CreateDirectory(_options.ErrorsDir);
        if (_options.CopyToArchive)
        {
            Directory.CreateDirectory(_options.ArchiveDir);
        }
    }

    private static string SanitizeForFileName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            sanitized.Append(invalid.Contains(ch) ? '_' : ch);
        }
        return sanitized.ToString();
    }

    private static string BuildPhysicalSliceKey(string? warehouseCode, string? binCode)
    {
        if (!string.IsNullOrWhiteSpace(warehouseCode) && !string.IsNullOrWhiteSpace(binCode))
        {
            return $"{warehouseCode}-{binCode}";
        }

        return binCode ?? warehouseCode ?? "UNKNOWN";
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.IndexOfAny(new[] { ',', '\"', '\r', '\n' }) >= 0;
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }

    private sealed record GenerationResult(string FilePath, string? ErrorFilePath);

    private sealed record BalanceRecord(
        Guid ItemId,
        Guid? WarehouseId,
        string? WarehouseCode,
        Guid? BinId,
        string? BinCode,
        Guid? BatchId,
        string? BatchCode,
        string? ItemCode,
        string? BaseUoM,
        decimal QtyBase,
        decimal AdjValue,
        string? Quality,
        DateTime? ExpDate);

    private readonly record struct AdjustmentKey(Guid ItemId, Guid? WarehouseId, Guid? BinId, Guid? BatchId);

    private sealed record AgnumRow(
        DateTimeOffset ExportAtUtc,
        string SliceType,
        string SliceKey,
        string? ItemCode,
        string? BaseUoM,
        decimal QtyBase,
        decimal AdjValue,
        string? BatchCode,
        string? LocationCode,
        string? Quality,
        DateTime? ExpDate);

    private sealed record ErrorRow(
        DateTimeOffset ExportAtUtc,
        string SliceType,
        string SliceKey,
        string? ItemCode,
        string Reason);
}
