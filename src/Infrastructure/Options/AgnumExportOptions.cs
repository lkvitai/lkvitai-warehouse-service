using System;
using System.IO;

namespace Lkvitai.Warehouse.Infrastructure.Options;

public sealed class AgnumExportOptions
{
    public string OutDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "exports", "agnum");
    public string ArchiveDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "exports", "agnum", "archive");
    public string ErrorsDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "exports", "agnum", "errors");
    public bool CopyToArchive { get; set; } = true;
    public string DailyRunAtUtc { get; set; } = "19:30";
}