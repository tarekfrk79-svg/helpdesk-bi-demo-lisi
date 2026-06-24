using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Demo;

public sealed record DashboardAlertTicketDto(
    int TicketId,
    string Title,
    TicketPriority Priority,
    string RequesterName,
    string? AssignedTechnicianName,
    DateTime DueAtUtc,
    bool IsOverdue,
    double? TechnicianResolutionHours);
