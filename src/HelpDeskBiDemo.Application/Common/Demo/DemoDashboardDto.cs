using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Demo;

public sealed record DemoDashboardDto(
    int CompanyId,
    string CompanyName,
    DemoRole ActiveRole,
    string ActiveRoleLabel,
    string? ActivePersonDisplayName,
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int OverdueTickets,
    int UnassignedTickets,
    double ResolutionRatePercent,
    double AverageResolutionHours,
    double AverageTechnicianResolutionHours,
    IReadOnlyList<CategoryCountDto> TicketsByCategory,
    IReadOnlyList<CategoryCountDto> TicketsByPriority,
    IReadOnlyList<TechnicianWorkloadDto> TechnicianWorkloads,
    IReadOnlyList<DashboardAlertTicketDto> OverdueTicketAlerts,
    IReadOnlyList<DashboardAlertTicketDto> UnassignedTicketAlerts,
    IReadOnlyList<TicketSummaryDto> RecentTickets);
