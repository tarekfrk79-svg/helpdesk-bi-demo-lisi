using HelpDeskBiDemo.Application.Common.Demo;
using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Tickets;

public sealed record TicketBoardDto(
    int CompanyId,
    string CompanyName,
    DemoRole ActiveRole,
    string ActiveRoleLabel,
    string? ActivePersonDisplayName,
    string? SearchTerm,
    TicketStatus? StatusFilter,
    TicketPriority? PriorityFilter,
    TicketCategory? CategoryFilter,
    int? AssignedTechnicianIdFilter,
    DateTime? CreatedFromDate,
    DateTime? CreatedToDate,
    bool OnlyMine,
    bool CanCreateTicket,
    bool CanManageTickets,
    IReadOnlyList<DemoPersonDto> TechnicianOptions,
    IReadOnlyList<TicketListItemDto> Tickets);
