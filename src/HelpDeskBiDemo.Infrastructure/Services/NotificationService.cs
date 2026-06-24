using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Notifications;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationHeaderSummaryDto?> GetHeaderSummaryAsync(
        int companyId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default)
    {
        var notificationQuery = BuildScopedNotificationQuery(companyId, role, personId);
        var unreadCount = await notificationQuery.CountAsync(x => !x.IsRead, cancellationToken);
        var liveAlertCount = await BuildLiveAlertItemsAsync(companyId, role, personId, cancellationToken);

        return new NotificationHeaderSummaryDto(unreadCount, liveAlertCount.Count);
    }

    public async Task<NotificationCenterDto?> GetCenterAsync(
        int companyId,
        DemoRole role,
        int? personId,
        string? activePersonDisplayName,
        CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == companyId && x.IsActive, cancellationToken);

        if (company is null)
        {
            return null;
        }

        var notifications = await BuildScopedNotificationQuery(companyId, role, personId)
            .OrderBy(x => x.IsRead)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(40)
            .Select(notification => new NotificationItemDto(
                notification.Id,
                notification.NotificationType,
                notification.Title,
                notification.Message,
                notification.ActionUrl,
                notification.CreatedAtUtc,
                notification.IsRead,
                false))
            .ToListAsync(cancellationToken);

        var liveAlerts = await BuildLiveAlertItemsAsync(companyId, role, personId, cancellationToken);

        return new NotificationCenterDto(
            BuildAudienceTitle(role, activePersonDisplayName),
            BuildAudienceDescription(role),
            notifications.Count(x => !x.IsRead),
            liveAlerts.Count,
            notifications,
            liveAlerts);
    }

    public async Task MarkAsReadAsync(
        int companyId,
        DemoRole role,
        int? personId,
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await BuildScopedNotificationQuery(companyId, role, personId)
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        notification.ReadAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(
        int companyId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await BuildScopedNotificationQuery(companyId, role, personId)
            .Where(x => !x.IsRead)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Domain.Entities.Notification> BuildScopedNotificationQuery(int companyId, DemoRole role, int? personId)
    {
        var query = _dbContext.Notifications
            .Where(x => x.CompanyId == companyId && x.RecipientRole == role)
            .AsQueryable();

        if (!personId.HasValue)
        {
            return query.Where(x => !x.RecipientPersonId.HasValue);
        }

        return query.Where(x => !x.RecipientPersonId.HasValue || x.RecipientPersonId == personId.Value);
    }

    private async Task<IReadOnlyList<NotificationItemDto>> BuildLiveAlertItemsAsync(
        int companyId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken)
    {
        var ticketsQuery = _dbContext.Tickets
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (role == DemoRole.CompanyAdmin)
        {
            return await BuildAdminLiveAlertsAsync(ticketsQuery, cancellationToken);
        }

        if (role == DemoRole.SupportTechnician)
        {
            if (!personId.HasValue)
            {
                return [];
            }

            ticketsQuery = ticketsQuery.Where(x => x.AssignedTechnicianId == personId.Value || x.AssignedTechnicianId == null);
            return await BuildTechnicianLiveAlertsAsync(ticketsQuery, personId.Value, cancellationToken);
        }

        if (role == DemoRole.EndUser)
        {
            if (!personId.HasValue)
            {
                return [];
            }

            ticketsQuery = ticketsQuery.Where(x => x.CreatedByPersonId == personId.Value);
            return await BuildEndUserLiveAlertsAsync(ticketsQuery, cancellationToken);
        }

        return [];
    }

    private async Task<IReadOnlyList<NotificationItemDto>> BuildAdminLiveAlertsAsync(
        IQueryable<Domain.Entities.Ticket> ticketsQuery,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var tickets = await ticketsQuery
            .Select(ticket => new
            {
                ticket.Id,
                ticket.Title,
                ticket.Priority,
                ticket.Status,
                ticket.CreatedAtUtc,
                ticket.AssignedTechnicianId,
                AssignedTechnicianName = ticket.AssignedTechnician != null ? ticket.AssignedTechnician.FullName : null
            })
            .ToListAsync(cancellationToken);

        var alerts = tickets
            .Where(ticket => TicketSlaPolicy.IsOverdue(ticket.Priority, ticket.CreatedAtUtc, ticket.Status, utcNow))
            .OrderBy(ticket => TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc))
            .Take(4)
            .Select(ticket => new NotificationItemDto(
                null,
                NotificationType.SlaAlert,
                "Ticket en retard SLA",
                $"Le ticket \"{ticket.Title}\" a depasse son echeance et demande une action admin.",
                $"/Tickets/Details/{ticket.Id}",
                TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc),
                false,
                true))
            .ToList();

        alerts.AddRange(
            tickets
                .Where(ticket =>
                    !ticket.AssignedTechnicianId.HasValue &&
                    ticket.Status != TicketStatus.Resolved &&
                    ticket.Status != TicketStatus.Closed &&
                    (ticket.Priority == TicketPriority.Urgent || ticket.Priority == TicketPriority.High))
                .OrderByDescending(ticket => (int)ticket.Priority)
                .ThenBy(ticket => ticket.CreatedAtUtc)
                .Take(3)
                .Select(ticket => new NotificationItemDto(
                    null,
                    NotificationType.SlaAlert,
                    "Ticket prioritaire non assigne",
                    $"Le ticket \"{ticket.Title}\" attend encore un technicien.",
                    $"/Tickets/Details/{ticket.Id}",
                    ticket.CreatedAtUtc,
                    false,
                    true)));

        return alerts
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    private async Task<IReadOnlyList<NotificationItemDto>> BuildTechnicianLiveAlertsAsync(
        IQueryable<Domain.Entities.Ticket> ticketsQuery,
        int personId,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var tickets = await ticketsQuery
            .Select(ticket => new
            {
                ticket.Id,
                ticket.Title,
                ticket.Priority,
                ticket.Status,
                ticket.CreatedAtUtc,
                ticket.AssignedTechnicianId
            })
            .ToListAsync(cancellationToken);

        var alerts = tickets
            .Where(ticket =>
                ticket.AssignedTechnicianId == personId &&
                TicketSlaPolicy.IsOverdue(ticket.Priority, ticket.CreatedAtUtc, ticket.Status, utcNow))
            .OrderBy(ticket => TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc))
            .Take(4)
            .Select(ticket => new NotificationItemDto(
                null,
                NotificationType.SlaAlert,
                "Ticket de ta file en retard",
                $"Le ticket \"{ticket.Title}\" depasse son delai cible.",
                $"/Tickets/Details/{ticket.Id}",
                TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc),
                false,
                true))
            .ToList();

        alerts.AddRange(
            tickets
                .Where(ticket =>
                    !ticket.AssignedTechnicianId.HasValue &&
                    ticket.Status == TicketStatus.New &&
                    (ticket.Priority == TicketPriority.Urgent || ticket.Priority == TicketPriority.High))
                .OrderByDescending(ticket => (int)ticket.Priority)
                .ThenBy(ticket => ticket.CreatedAtUtc)
                .Take(3)
                .Select(ticket => new NotificationItemDto(
                    null,
                    NotificationType.SlaAlert,
                    "Ticket prioritaire disponible",
                    $"Le ticket \"{ticket.Title}\" est disponible pour prise en charge.",
                    $"/Tickets/Details/{ticket.Id}",
                    ticket.CreatedAtUtc,
                    false,
                    true)));

        return alerts
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    private async Task<IReadOnlyList<NotificationItemDto>> BuildEndUserLiveAlertsAsync(
        IQueryable<Domain.Entities.Ticket> ticketsQuery,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var tickets = await ticketsQuery
            .Select(ticket => new
            {
                ticket.Id,
                ticket.Title,
                ticket.Priority,
                ticket.Status,
                ticket.CreatedAtUtc,
                ticket.AssignedTechnicianId
            })
            .ToListAsync(cancellationToken);

        return tickets
            .Where(ticket =>
                ticket.Status != TicketStatus.Resolved &&
                ticket.Status != TicketStatus.Closed &&
                TicketSlaPolicy.IsOverdue(ticket.Priority, ticket.CreatedAtUtc, ticket.Status, utcNow))
            .OrderBy(ticket => TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc))
            .Take(4)
            .Select(ticket => new NotificationItemDto(
                null,
                NotificationType.SlaAlert,
                "Suivi ticket necessaire",
                !ticket.AssignedTechnicianId.HasValue
                    ? $"Ton ticket \"{ticket.Title}\" attend encore une prise en charge."
                    : $"Ton ticket \"{ticket.Title}\" depasse le delai cible et merite une relance.",
                $"/Tickets/Details/{ticket.Id}",
                TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc),
                false,
                true))
            .ToList();
    }

    private static string BuildAudienceTitle(DemoRole role, string? activePersonDisplayName)
    {
        if (!string.IsNullOrWhiteSpace(activePersonDisplayName))
        {
            return $"Notifications - {activePersonDisplayName}";
        }

        return $"Notifications - {DemoRoleFormatter.ToDisplayLabel(role)}";
    }

    private static string BuildAudienceDescription(DemoRole role)
    {
        return role switch
        {
            DemoRole.CompanyAdmin => "Tu retrouves ici les nouveaux tickets, les resolutions recentes et les alertes de supervision.",
            DemoRole.SupportTechnician => "Tu retrouves ici les tickets assignes, les reponses utilisateur et les alertes SLA utiles a ton suivi.",
            DemoRole.EndUser => "Tu retrouves ici les evolutions de tes tickets et les alertes de suivi importantes.",
            _ => "Centre de notifications de la demo."
        };
    }
}
