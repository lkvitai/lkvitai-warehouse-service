using Lkvitai.Warehouse.Domain.Entities;

namespace Lkvitai.Warehouse.Application.Exports.Contracts;

public sealed record AgnumExportRequest(
    string SliceType,
    string SliceKey,
    string Format = ExportJobFormat.Csv)
{
    public static class Types
    {
        public const string Physical = "Physical";
        public const string Logical = "Logical";
        public const string Total = "Total";
    }
}
