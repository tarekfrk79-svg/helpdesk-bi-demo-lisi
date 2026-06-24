using System.Globalization;
using System.Text;
using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Exports;
using HelpDeskBiDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class CsvExportService : ICsvExportService
{
    private readonly ApplicationDbContext _dbContext;

    public CsvExportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CsvExportResult?> ExportCompanyTicketsAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken);

        if (company is null)
        {
            return null;
        }

        var tickets = await _dbContext.Tickets
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(ticket => new
            {
                CompanyName = ticket.Company!.Name,
                ticket.Id,
                ticket.Title,
                ticket.Description,
                ticket.Category,
                ticket.Priority,
                ticket.Status,
                Requester = ticket.CreatedByPerson!.FullName,
                ticket.AssignedTechnicianId,
                AssignedTechnician = ticket.AssignedTechnician != null ? ticket.AssignedTechnician.FullName : string.Empty,
                ticket.CreatedAtUtc,
                ticket.AssignedAtUtc,
                ticket.UpdatedAtUtc,
                ticket.ResolvedAtUtc,
                CommentCount = ticket.Comments.Count,
                ActivityCount = ticket.Activities.Count
            })
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            "Company,TicketId,Title,Description,Category,Priority,Status,Requester,AssignedTechnician,CreatedAtUtc,AssignedAtUtc,DueAtUtc,UpdatedAtUtc,ResolvedAtUtc,ResolutionHours,TechnicianResolutionHours,IsOverdue,CommentCount,ActivityCount");

        foreach (var ticket in tickets)
        {
            var resolutionHours = ticket.ResolvedAtUtc.HasValue
                ? Math.Round((ticket.ResolvedAtUtc.Value - ticket.CreatedAtUtc).TotalHours, 2)
                : (double?)null;
            var dueAtUtc = TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc);
            var technicianResolutionHours = TicketSlaPolicy.GetTechnicianResolutionHours(ticket.AssignedAtUtc, ticket.ResolvedAtUtc);
            var isOverdue = TicketSlaPolicy.IsOverdue(ticket.Priority, ticket.CreatedAtUtc, ticket.Status, DateTime.UtcNow);

            var values = new[]
            {
                ticket.CompanyName,
                ticket.Id.ToString(CultureInfo.InvariantCulture),
                ticket.Title,
                ticket.Description,
                TicketLabelFormatter.ToDisplayLabel(ticket.Category),
                TicketLabelFormatter.ToDisplayLabel(ticket.Priority),
                TicketLabelFormatter.ToDisplayLabel(ticket.Status),
                ticket.Requester,
                ticket.AssignedTechnician,
                ticket.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                ticket.AssignedAtUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                dueAtUtc.ToString("O", CultureInfo.InvariantCulture),
                ticket.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                ticket.ResolvedAtUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                resolutionHours?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                technicianResolutionHours?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                isOverdue ? "true" : "false",
                ticket.CommentCount.ToString(CultureInfo.InvariantCulture),
                ticket.ActivityCount.ToString(CultureInfo.InvariantCulture)
            };

            builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
        }

        var fileName = $"helpdesk-bi-demo-{Slugify(company.Name)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return new CsvExportResult(fileName, builder.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string Slugify(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();

        return new string(chars).Trim('-');
    }
}
