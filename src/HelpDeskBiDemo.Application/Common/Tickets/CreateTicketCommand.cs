using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Tickets;

public sealed record CreateTicketCommand(
    string Title,
    string Description,
    TicketCategory Category,
    TicketPriority Priority);
