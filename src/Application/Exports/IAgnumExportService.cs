using Lkvitai.Warehouse.Application.Exports.Contracts;
using Lkvitai.Warehouse.Domain.Entities;

namespace Lkvitai.Warehouse.Application.Exports;

public interface IAgnumExportService
{
    Task<ExportJob> RunAsync(AgnumExportRequest request, CancellationToken cancellationToken = default);
}
