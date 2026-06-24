using HelpDeskBiDemo.Application.Common.Exports;

namespace HelpDeskBiDemo.Application.Abstractions;

public interface ICsvExportService
{
    Task<CsvExportResult?> ExportCompanyTicketsAsync(int companyId, CancellationToken cancellationToken = default);
}
