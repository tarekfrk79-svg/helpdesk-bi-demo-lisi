using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Demo;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class DemoCompanyService : IDemoCompanyService
{
    private readonly ApplicationDbContext _dbContext;

    public DemoCompanyService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompanyContextDto?> GetCompanyContextAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .Where(x => x.Id == companyId && x.IsActive)
            .Select(x => new CompanyContextDto(x.Id, x.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DemoPersonDto?> GetDefaultAdminAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Role == DemoRole.CompanyAdmin && x.IsActive)
            .OrderBy(x => x.Id)
            .Select(ToPersonDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DemoPersonDto>> GetPeopleByRoleAsync(
        int companyId,
        DemoRole role,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Role == role && x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(ToPersonDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<DemoPersonDto?> GetPersonAsync(
        int companyId,
        int personId,
        DemoRole role,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId &&
                        x.Id == personId &&
                        x.Role == role &&
                        x.IsActive)
            .Select(ToPersonDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DemoDashboardDto?> GetDashboardAsync(
        int companyId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == companyId && x.IsActive, cancellationToken);

        if (company is null)
        {
            return null;
        }

        var activePerson = personId is null
            ? null
            : await _dbContext.DemoPeople
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Id == personId.Value && x.IsActive)
                .Select(ToPersonDto())
                .FirstOrDefaultAsync(cancellationToken);

        var ticketsQuery = _dbContext.Tickets
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (role == DemoRole.SupportTechnician && personId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(x => x.AssignedTechnicianId == personId.Value || x.AssignedTechnicianId == null);
        }

        if (role == DemoRole.EndUser && personId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(x => x.CreatedByPersonId == personId.Value);
        }

        var utcNow = DateTime.UtcNow;

        var rawTickets = await ticketsQuery
            .Select(ticket => new
            {
                ticket.Id,
                ticket.Title,
                ticket.Status,
                ticket.Priority,
                ticket.Category,
                ticket.CreatedAtUtc,
                ticket.AssignedAtUtc,
                ticket.ResolvedAtUtc,
                RequesterName = ticket.CreatedByPerson!.FullName,
                ticket.AssignedTechnicianId,
                AssignedTechnicianName = ticket.AssignedTechnician != null ? ticket.AssignedTechnician.FullName : null,
                CommentCount = ticket.Comments.Count
            })
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var tickets = rawTickets
            .Select(ticket =>
            {
                var dueAtUtc = TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc);
                return new DashboardTicketSnapshot(
                    ticket.Id,
                    ticket.Title,
                    ticket.Status,
                    ticket.Priority,
                    ticket.Category,
                    ticket.CreatedAtUtc,
                    ticket.AssignedAtUtc,
                    ticket.ResolvedAtUtc,
                    ticket.RequesterName,
                    ticket.AssignedTechnicianId,
                    ticket.AssignedTechnicianName,
                    ticket.CommentCount,
                    dueAtUtc,
                    TicketSlaPolicy.IsOverdue(ticket.Priority, ticket.CreatedAtUtc, ticket.Status, utcNow),
                    TicketSlaPolicy.GetTechnicianResolutionHours(ticket.AssignedAtUtc, ticket.ResolvedAtUtc));
            })
            .ToList();

        var totalTickets = tickets.Count;
        var openTickets = tickets.Count(x => x.Status == TicketStatus.New);
        var inProgressTickets = tickets.Count(x => x.Status == TicketStatus.InProgress);
        var resolvedTickets = tickets.Count(x => x.Status == TicketStatus.Resolved);
        var overdueTickets = tickets.Count(x => x.IsOverdue);
        var unassignedTickets = tickets.Count(x =>
            !x.AssignedTechnicianId.HasValue &&
            x.Status != TicketStatus.Resolved &&
            x.Status != TicketStatus.Closed);

        var resolutionHours = tickets
            .Where(x => x.ResolvedAtUtc.HasValue)
            .Select(x => (x.ResolvedAtUtc!.Value - x.CreatedAtUtc).TotalHours)
            .ToList();

        var technicianResolutionHours = tickets
            .Where(x => x.TechnicianResolutionHours.HasValue)
            .Select(x => x.TechnicianResolutionHours!.Value)
            .ToList();

        var categoryCounts = tickets
            .GroupBy(x => x.Category)
            .Select(group => new CategoryCountDto(TicketLabelFormatter.ToDisplayLabel(group.Key), group.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var priorityCounts = tickets
            .GroupBy(x => x.Priority)
            .Select(group => new CategoryCountDto(TicketLabelFormatter.ToDisplayLabel(group.Key), group.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var technicianWorkloads = role == DemoRole.CompanyAdmin
            ? await BuildTechnicianWorkloadsAsync(companyId, tickets, cancellationToken)
            : [];

        var overdueAlerts = role == DemoRole.CompanyAdmin
            ? tickets
                .Where(x => x.IsOverdue)
                .OrderBy(x => x.DueAtUtc)
                .Take(5)
                .Select(ToDashboardAlertDto)
                .ToList()
            : [];

        var unassignedAlerts = role == DemoRole.CompanyAdmin
            ? tickets
                .Where(x => !x.AssignedTechnicianId.HasValue && x.Status != TicketStatus.Resolved && x.Status != TicketStatus.Closed)
                .OrderByDescending(x => (int)x.Priority)
                .ThenBy(x => x.CreatedAtUtc)
                .Take(5)
                .Select(ToDashboardAlertDto)
                .ToList()
            : [];

        var recentTickets = tickets
            .Take(6)
            .Select(ticket => new TicketSummaryDto(
                ticket.Id,
                ticket.Title,
                ticket.Status,
                ticket.Priority,
                ticket.Category,
                ticket.RequesterName,
                ticket.AssignedTechnicianId,
                ticket.AssignedTechnicianName,
                ticket.CreatedAtUtc,
                ticket.DueAtUtc,
                ticket.IsOverdue,
                ticket.ResolvedAtUtc,
                ticket.TechnicianResolutionHours,
                ticket.CommentCount))
            .ToList();

        return new DemoDashboardDto(
            company.Id,
            company.Name,
            role,
            DemoRoleFormatter.ToDisplayLabel(role),
            activePerson?.DisplayLabel,
            totalTickets,
            openTickets,
            inProgressTickets,
            resolvedTickets,
            overdueTickets,
            unassignedTickets,
            totalTickets == 0 ? 0 : Math.Round(tickets.Count(x => x.Status == TicketStatus.Resolved || x.Status == TicketStatus.Closed) * 100d / totalTickets, 1),
            resolutionHours.Count == 0 ? 0 : Math.Round(resolutionHours.Average(), 1),
            technicianResolutionHours.Count == 0 ? 0 : Math.Round(technicianResolutionHours.Average(), 1),
            categoryCounts,
            priorityCounts,
            technicianWorkloads,
            overdueAlerts,
            unassignedAlerts,
            recentTickets);
    }

    private async Task<IReadOnlyList<TechnicianWorkloadDto>> BuildTechnicianWorkloadsAsync(
        int companyId,
        IReadOnlyList<DashboardTicketSnapshot> tickets,
        CancellationToken cancellationToken)
    {
        var technicians = await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Role == DemoRole.SupportTechnician && x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new { x.Id, x.FullName })
            .ToListAsync(cancellationToken);

        return technicians
            .Select(technician =>
            {
                var assignedTickets = tickets
                    .Where(ticket => ticket.AssignedTechnicianId == technician.Id)
                    .ToList();

                var technicianHours = assignedTickets
                    .Where(ticket => ticket.TechnicianResolutionHours.HasValue)
                    .Select(ticket => ticket.TechnicianResolutionHours!.Value)
                    .ToList();

                return new TechnicianWorkloadDto(
                    technician.Id,
                    technician.FullName,
                    assignedTickets.Count(ticket => ticket.Status == TicketStatus.New || ticket.Status == TicketStatus.InProgress),
                    assignedTickets.Count(ticket => ticket.Status == TicketStatus.InProgress),
                    assignedTickets.Count(ticket => ticket.IsOverdue),
                    assignedTickets.Count(ticket => ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed),
                    technicianHours.Count == 0 ? 0 : Math.Round(technicianHours.Average(), 1));
            })
            .ToList();
    }

    private static DashboardAlertTicketDto ToDashboardAlertDto(DashboardTicketSnapshot ticket)
    {
        return new DashboardAlertTicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Priority,
            ticket.RequesterName,
            ticket.AssignedTechnicianName,
            ticket.DueAtUtc,
            ticket.IsOverdue,
            ticket.TechnicianResolutionHours);
    }

    private static System.Linq.Expressions.Expression<Func<Domain.Entities.DemoPerson, DemoPersonDto>> ToPersonDto() =>
        person => new DemoPersonDto(
            person.Id,
            person.FullName,
            person.JobTitle,
            person.Department,
            person.Role,
            string.IsNullOrWhiteSpace(person.Department)
                ? $"{person.FullName} - {person.JobTitle}"
                : $"{person.FullName} - {person.Department}");

    private sealed record DashboardTicketSnapshot(
        int Id,
        string Title,
        TicketStatus Status,
        TicketPriority Priority,
        TicketCategory Category,
        DateTime CreatedAtUtc,
        DateTime? AssignedAtUtc,
        DateTime? ResolvedAtUtc,
        string RequesterName,
        int? AssignedTechnicianId,
        string? AssignedTechnicianName,
        int CommentCount,
        DateTime DueAtUtc,
        bool IsOverdue,
        double? TechnicianResolutionHours);
}
