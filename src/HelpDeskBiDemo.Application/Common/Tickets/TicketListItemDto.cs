using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Tickets;

public sealed record TicketListItemDto(
    int TicketId,
    string Title,
    TicketCategory Category,
    TicketPriority Priority,
    TicketStatus Status,
    string RequesterName,
    int? AssignedTechnicianId,
    string? AssignedTechnicianName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? AssignedAtUtc,
    DateTime DueAtUtc,
    bool IsOverdue,
    double? TechnicianResolutionHours,
    int CommentCount);
