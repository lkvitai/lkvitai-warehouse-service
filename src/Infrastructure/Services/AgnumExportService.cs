using System.Globalization;
using System.Text;
using Lkvitai.Warehouse.Infrastructure.Export;
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
    private readonly SftpUploader _sftpUploader;

    public AgnumExportService(
        WarehouseDbContext db,
        IOptions<AgnumExportOptions> options,
        ILogger<AgnumExportService> logger,
        SftpUploader sftpUploader)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
        _sftpUploader = sftpUploader;
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

            job.Status = result.Ok ? ExportJobStatus.Succeeded : ExportJobStatus.Failed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.FilePath = result.FilePath;
            job.ErrorFilePath = result.ErrorFilePath;
            job.ExportedAtUtc = utcNow;
            job.ErrorMessage = result.Ok ? null : FormatValidationError(result.Errors);

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

    private static string FormatValidationError(IReadOnlyList<CsvValidation.RowError> errors)
    {
        if (errors.Count == 0)
        {
            return "CSV validation failed";
        }

        var first = errors[0];
        var suffix = errors.Count > 1 ? $" (+{errors.Count - 1} more)" : string.Empty;
        return $"CSV validation failed at line {first.LineNo}: {first.Message}{suffix}";
    }
    private static bool IsSupportedSlice(string sliceType) =>
        string.Equals(sliceType, AgnumExportRequest.Types.Physical, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(sliceType, AgnumExportRequest.Types.Total, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(sliceType, AgnumExportRequest.Types.Logical, StringComparison.OrdinalIgnoreCase);

    private async Task<GenerationResult> GenerateAsync(AgnumExportRequest request, DateTimeOffset exportAtUtc, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var sanitizedSliceKey = SanitizeForFileName(request.SliceKey);
        var timestamp = exportAtUtc.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var fileBaseName = $"stock_{request.SliceType}_{sanitizedSliceKey}_{timestamp}";
        var outDir = Path.Combine(Path.GetTempPath(), "agnum-exports");
        Directory.CreateDirectory(outDir);

        IReadOnlyList<AgnumRow> rows;
        if (request.SliceType.Equals(AgnumExportRequest.Types.Physical, StringComparison.OrdinalIgnoreCase))
        {
            var data = await LoadDataAsync(request, ct);
            rows = BuildPhysicalRows(data, exportAtUtc);
        }
        else if (request.SliceType.Equals(AgnumExportRequest.Types.Total, StringComparison.OrdinalIgnoreCase))
        {
            var data = await LoadDataAsync(request, ct);
            rows = BuildTotalRows(data, request.SliceKey, exportAtUtc);
        }
        else
        {
            rows = await QueryLogicalAsync(request.SliceKey, exportAtUtc, ct);
        }

        ct.ThrowIfCancellationRequested();

        var lines = BuildCsvLines(rows);
        var (ok, errors) = CsvValidation.Validate(lines);

        var csvPath = Path.Combine(outDir, fileBaseName + ".csv");
        await File.WriteAllLinesAsync(csvPath, lines, ct);

        string? errorPath = null;
        if (!ok)
        {
            var errorCsv = CsvValidation.BuildErrorsCsv(errors);
            errorPath = Path.Combine(outDir, fileBaseName + "_errors.csv");
            await File.WriteAllTextAsync(errorPath, errorCsv, ct);
            _logger.LogWarning("Agnum export validation errors for {SliceType}:{SliceKey} - {ErrorCount} issues", request.SliceType, request.SliceKey, errors.Count);
        }

        if (_options.CopyToArchive)
        {
            Directory.CreateDirectory(_options.ArchiveDir);
            File.Copy(csvPath, Path.Combine(_options.ArchiveDir, Path.GetFileName(csvPath)), overwrite: true);
            if (errorPath != null)
            {
                Directory.CreateDirectory(_options.ErrorsDir);
                File.Copy(errorPath, Path.Combine(_options.ErrorsDir, Path.GetFileName(errorPath)), overwrite: true);
            }
        }

        if (ok)
        {
            await _sftpUploader.UploadAsync(csvPath, Path.GetFileName(csvPath), ct);
        }

        return new GenerationResult(ok, csvPath, errorPath, errors);
    }

    private async Task<IReadOnlyList<BalanceRecord>> LoadDataAsync(AgnumExportRequest request, CancellationToken ct)
    {
        var isPhysical = request.SliceType.Equals(AgnumExportRequest.Types.Physical, StringComparison.OrdinalIgnoreCase);

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

        if (isPhysical)
        {
            baseQuery = baseQuery.Where(x => x.balance.BinId != null && x.bin != null);
        }

        var records = await baseQuery
            .Select(x => new BalanceRecord(
                x.balance.ItemId,
                x.balance.WarehousePhysicalId,
                x.warehouse != null ? x.warehouse.Code : null,
                x.balance.BinId,
                x.bin != null ? x.bin.Code : null,
                x.balance.BatchId,
                x.batch != null ? x.batch.BatchNo : null,
                x.item.Sku,
                x.item.UomBase,
                x.balance.QtyBase,
                0m,
                x.batch != null ? x.batch.Quality.ToString() : null,
                x.batch != null ? x.batch.ExpDate : (DateTime?)null))
            .ToListAsync(ct);

        var adjustments = await LoadAdjustmentRecordsAsync(request, ct);

        if (records.Count == 0 && adjustments.Count == 0)
        {
            return Array.Empty<BalanceRecord>();
        }

        var lookup = adjustments.ToDictionary(x => x.Key);
        var merged = new List<BalanceRecord>(records.Count + lookup.Count);

        foreach (var record in records)
        {
            var key = new AdjustmentKey(record.ItemId, record.WarehouseId, record.BinId, record.BatchId);
            if (lookup.Remove(key, out var adj))
            {
                merged.Add(record with { AdjValue = adj.Sum });
            }
            else
            {
                merged.Add(record);
            }
        }

        foreach (var adj in lookup.Values)
        {
            merged.Add(new BalanceRecord(
                adj.Key.ItemId,
                adj.Key.WarehouseId,
                adj.WarehouseCode,
                adj.Key.BinId,
                adj.BinCode,
                adj.Key.BatchId,
                adj.BatchCode,
                adj.ItemCode,
                adj.BaseUoM,
                0m,
                adj.Sum,
                adj.Quality,
                adj.ExpDate));
        }

        return merged;
    }

    private async Task<List<AdjustmentRecord>> LoadAdjustmentRecordsAsync(AgnumExportRequest request, CancellationToken ct)
    {
        if (request.SliceType.Equals(AgnumExportRequest.Types.Physical, StringComparison.OrdinalIgnoreCase))
        {
            return await LoadPhysicalAdjustmentRecordsAsync(request.SliceKey, ct);
        }

        if (request.SliceType.Equals(AgnumExportRequest.Types.Total, StringComparison.OrdinalIgnoreCase))
        {
            return await LoadTotalAdjustmentRecordsAsync(request.SliceKey, ct);
        }

        return new List<AdjustmentRecord>();
    }

    private async Task<List<AdjustmentRecord>> LoadPhysicalAdjustmentRecordsAsync(string sliceKey, CancellationToken ct)
    {
        var query =
            from v in _db.ValueAdjustments.AsNoTracking()
            join item in _db.Items.AsNoTracking() on v.ItemId equals item.Id
            join warehouse in _db.WarehousePhysicals.AsNoTracking() on v.WarehousePhysicalId equals warehouse.Id into whJoin
            from warehouse in whJoin.DefaultIfEmpty()
            join bin in _db.Bins.AsNoTracking() on v.BinId equals bin.Id into binJoin
            from bin in binJoin.DefaultIfEmpty()
            join batch in _db.StockBatches.AsNoTracking() on v.BatchId equals batch.Id into batchJoin
            from batch in batchJoin.DefaultIfEmpty()
            select new { v, item, warehouse, bin, batch };

        if (!string.Equals(sliceKey, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.warehouse != null && x.warehouse.Code == sliceKey);
        }

        var grouped = await query
            .GroupBy(x => new
            {
                x.v.ItemId,
                x.v.WarehousePhysicalId,
                x.v.BinId,
                x.v.BatchId,
                x.item.Sku,
                x.item.UomBase,
                WarehouseCode = x.warehouse != null ? x.warehouse.Code : null,
                BinCode = x.bin != null ? x.bin.Code : null,
                BatchCode = x.batch != null ? x.batch.BatchNo : null,
                Quality = x.batch != null ? (BatchQuality?)x.batch.Quality : null,
                ExpDate = x.batch != null ? x.batch.ExpDate : (DateTime?)null
            })
            .Select(g => new
            {
                g.Key.ItemId,
                g.Key.WarehousePhysicalId,
                g.Key.BinId,
                g.Key.BatchId,
                g.Key.Sku,
                g.Key.UomBase,
                g.Key.WarehouseCode,
                g.Key.BinCode,
                g.Key.BatchCode,
                g.Key.Quality,
                g.Key.ExpDate,
                Sum = g.Sum(x => x.v.DeltaValue)
            })
            .ToListAsync(ct);

        return grouped
            .Where(x => x.Sum != 0m)
            .Select(x => new AdjustmentRecord(
                new AdjustmentKey(x.ItemId, x.WarehousePhysicalId, x.BinId, x.BatchId),
                x.Sum,
                x.WarehouseCode,
                x.BinCode,
                x.BatchCode,
                x.Sku,
                x.UomBase,
                x.Quality?.ToString(),
                x.ExpDate))
            .ToList();
    }

    private async Task<List<AdjustmentRecord>> LoadTotalAdjustmentRecordsAsync(string sliceKey, CancellationToken ct)
    {
        var query =
            from v in _db.ValueAdjustments.AsNoTracking()
            join item in _db.Items.AsNoTracking() on v.ItemId equals item.Id
            join warehouse in _db.WarehousePhysicals.AsNoTracking() on v.WarehousePhysicalId equals warehouse.Id into whJoin
            from warehouse in whJoin.DefaultIfEmpty()
            select new { v, item, warehouse };

        if (!string.Equals(sliceKey, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.warehouse != null && x.warehouse.Code == sliceKey);
        }

        var grouped = await query
            .GroupBy(x => new { x.v.ItemId, x.item.Sku, x.item.UomBase })
            .Select(g => new
            {
                g.Key.ItemId,
                g.Key.Sku,
                g.Key.UomBase,
                Sum = g.Sum(x => x.v.DeltaValue)
            })
            .ToListAsync(ct);

        return grouped
            .Where(x => x.Sum != 0m)
            .Select(x => new AdjustmentRecord(
                new AdjustmentKey(x.ItemId, null, null, null),
                x.Sum,
                WarehouseCode: null,
                BinCode: null,
                BatchCode: null,
                x.Sku,
                x.UomBase,
                Quality: null,
                ExpDate: null))
            .ToList();
    }


    private async Task<IReadOnlyList<AgnumRow>> QueryLogicalAsync(string sliceKey, DateTimeOffset exportAtUtc, CancellationToken ct)
    {
        var includeAll = string.Equals(sliceKey, "ALL", StringComparison.OrdinalIgnoreCase);

        var balanceQuery =
            from balance in _db.StockBalances.AsNoTracking()
            where balance.WarehouseLogicalId != null
            join logical in _db.WarehouseLogicals.AsNoTracking() on balance.WarehouseLogicalId equals logical.Id
            join item in _db.Items.AsNoTracking() on balance.ItemId equals item.Id
            join batch in _db.StockBatches.AsNoTracking() on balance.BatchId equals batch.Id into batchJoin
            from batch in batchJoin.DefaultIfEmpty()
            select new { balance, logical, item, batch };

        if (!includeAll)
        {
            balanceQuery = balanceQuery.Where(x => x.logical.Code == sliceKey);
        }

        var balanceRows = await balanceQuery
            .Select(x => new
            {
                x.balance.ItemId,
                LogicalId = x.logical.Id,
                x.logical.Code,
                x.balance.BatchId,
                BatchCode = x.batch != null ? x.batch.BatchNo : null,
                Quality = x.batch != null ? (BatchQuality?)x.batch.Quality : null,
                ExpDate = x.batch != null ? x.batch.ExpDate : (DateTime?)null,
                x.item.Sku,
                x.item.UomBase,
                x.balance.QtyBase
            })
            .ToListAsync(ct);

        var balanceAggregates = balanceRows
            .GroupBy(x => new { x.ItemId, x.LogicalId, x.Code, x.BatchId, x.BatchCode, x.Quality, x.ExpDate, x.Sku, x.UomBase })
            .Select(g => new LogicalBalance(
                new LogicalKey(g.Key.ItemId, g.Key.LogicalId, g.Key.BatchId),
                g.Key.Code,
                g.Key.Sku,
                g.Key.UomBase,
                g.Key.BatchCode,
                g.Key.Quality?.ToString(),
                g.Key.ExpDate,
                g.Sum(x => x.QtyBase)))
            .ToList();

        var adjustmentQuery =
            from v in _db.ValueAdjustments.AsNoTracking()
            where v.WarehouseLogicalId != null
            join logical in _db.WarehouseLogicals.AsNoTracking() on v.WarehouseLogicalId equals logical.Id
            join item in _db.Items.AsNoTracking() on v.ItemId equals item.Id
            join batch in _db.StockBatches.AsNoTracking() on v.BatchId equals batch.Id into batchJoin
            from batch in batchJoin.DefaultIfEmpty()
            select new { v, logical, item, batch };

        if (!includeAll)
        {
            adjustmentQuery = adjustmentQuery.Where(x => x.logical.Code == sliceKey);
        }

        var adjustmentRows = await adjustmentQuery
            .GroupBy(x => new
            {
                x.v.ItemId,
                LogicalId = x.logical.Id,
                x.logical.Code,
                x.v.BatchId,
                BatchCode = x.batch != null ? x.batch.BatchNo : null,
                Quality = x.batch != null ? (BatchQuality?)x.batch.Quality : null,
                ExpDate = x.batch != null ? x.batch.ExpDate : (DateTime?)null,
                x.item.Sku,
                x.item.UomBase
            })
            .Select(g => new
            {
                g.Key.ItemId,
                g.Key.LogicalId,
                g.Key.Code,
                g.Key.BatchId,
                g.Key.BatchCode,
                g.Key.Quality,
                g.Key.ExpDate,
                g.Key.Sku,
                g.Key.UomBase,
                Sum = g.Sum(x => x.v.DeltaValue)
            })
            .ToListAsync(ct);

        var adjustments = adjustmentRows
            .Where(x => x.Sum != 0m)
            .Select(x => new LogicalAdjustment(
                new LogicalKey(x.ItemId, x.LogicalId, x.BatchId),
                x.Code,
                x.Sku,
                x.UomBase,
                x.BatchCode,
                x.Quality?.ToString(),
                x.ExpDate,
                x.Sum))
            .ToDictionary(x => x.Key);

        var rows = new List<AgnumRow>(balanceAggregates.Count + adjustments.Count);

        foreach (var balance in balanceAggregates)
        {
            adjustments.Remove(balance.Key, out var adj);
            rows.Add(new AgnumRow(
                exportAtUtc,
                AgnumExportRequest.Types.Logical,
                balance.LogicalCode,
                balance.ItemCode,
                balance.BaseUoM,
                balance.QtyBase,
                balance.QtyBase,
                adj?.Sum ?? 0m,
                balance.BatchCode,
                balance.LogicalCode,
                balance.Quality,
                balance.ExpDate));
        }

        foreach (var adj in adjustments.Values)
        {
            rows.Add(new AgnumRow(
                exportAtUtc,
                AgnumExportRequest.Types.Logical,
                adj.LogicalCode,
                adj.ItemCode,
                adj.BaseUoM,
                0m,
                0m,
                adj.Sum,
                adj.BatchCode,
                adj.LogicalCode,
                adj.Quality,
                adj.ExpDate));
        }

        return rows;
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
                g.Sum(x => x.QtyBase),
                g.Sum(x => x.AdjValue),
                BatchCode: null,
                LocationCode: string.Empty,
                Quality: null,
                ExpDate: null))
            .ToList();
    }


    private static string[] BuildCsvLines(IReadOnlyList<AgnumRow> rows)
    {
        var lines = new string[rows.Count + 1];
        lines[0] = string.Join(',', CsvValidation.Header);

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            lines[i + 1] = string.Join(',',
                Escape(row.ExportAtUtc.ToString("o", CultureInfo.InvariantCulture)),
                Escape(row.SliceType),
                Escape(row.SliceKey),
                Escape(row.ItemCode),
                Escape(row.BaseUoM),
                Escape(row.QtyBase.ToString("0.####", CultureInfo.InvariantCulture)),
                Escape(row.DisplayQty.ToString("0.####", CultureInfo.InvariantCulture)),
                Escape(row.AdjValue.ToString("0.00", CultureInfo.InvariantCulture)),
                Escape(row.BatchCode),
                Escape(row.LocationCode),
                Escape(row.Quality),
                Escape(row.ExpDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        return lines;
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

        var needsQuotes = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }

    private sealed record GenerationResult(bool Ok, string FilePath, string? ErrorFilePath, IReadOnlyList<CsvValidation.RowError> Errors);

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

    private sealed record AdjustmentRecord(
        AdjustmentKey Key,
        decimal Sum,
        string? WarehouseCode,
        string? BinCode,
        string? BatchCode,
        string? ItemCode,
        string? BaseUoM,
        string? Quality,
        DateTime? ExpDate);

    private readonly record struct LogicalKey(Guid ItemId, Guid LogicalId, Guid? BatchId);

    private sealed record LogicalBalance(
        LogicalKey Key,
        string LogicalCode,
        string? ItemCode,
        string? BaseUoM,
        string? BatchCode,
        string? Quality,
        DateTime? ExpDate,
        decimal QtyBase);

    private sealed record LogicalAdjustment(
        LogicalKey Key,
        string LogicalCode,
        string? ItemCode,
        string? BaseUoM,
        string? BatchCode,
        string? Quality,
        DateTime? ExpDate,
        decimal Sum);

    private readonly record struct AdjustmentKey(Guid ItemId, Guid? WarehouseId, Guid? BinId, Guid? BatchId);

    private sealed record AgnumRow(
        DateTimeOffset ExportAtUtc,
        string SliceType,
        string SliceKey,
        string? ItemCode,
        string? BaseUoM,
        decimal QtyBase,
        decimal DisplayQty,
        decimal AdjValue,
        string? BatchCode,
        string? LocationCode,
        string? Quality,
        DateTime? ExpDate);

}
