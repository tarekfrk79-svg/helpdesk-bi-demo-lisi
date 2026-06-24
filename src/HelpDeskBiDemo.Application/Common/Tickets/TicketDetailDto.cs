using HelpDeskBiDemo.Application.Common.Demo;
using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Tickets;

public sealed record TicketDetailDto(
    int TicketId,
    int CompanyId,
    string CompanyName,
    DemoRole ActiveRole,
    string ActiveRoleLabel,
    string? ActivePersonDisplayName,
    string Title,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    TicketStatus Status,
    string RequesterName,
    int? AssignedTechnicianId,
    string? AssignedTechnicianName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? AssignedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime DueAtUtc,
    bool IsOverdue,
    int SlaTargetHours,
    double? TechnicianResolutionHours,
    double? TechnicianActiveHours,
    bool CanManageTicket,
    bool CanComment,
    bool CanSelfAssign,
    IReadOnlyList<DemoPersonDto> TechnicianOptions,
    IReadOnlyList<TicketActivityDto> Activities,
    IReadOnlyList<TicketCommentDto> Comments);
