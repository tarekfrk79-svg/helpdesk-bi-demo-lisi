using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Demo;

public sealed record TicketSummaryDto(
    int Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    TicketCategory Category,
    string RequesterName,
    int? AssignedTechnicianId,
    string? AssignedTechnicianName,
    DateTime CreatedAtUtc,
    DateTime DueAtUtc,
    bool IsOverdue,
    DateTime? ResolvedAtUtc,
    double? TechnicianResolutionHours,
    int CommentCount);
