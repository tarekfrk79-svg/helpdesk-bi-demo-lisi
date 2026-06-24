using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Tickets;

public sealed record ManageTicketCommand(
    TicketStatus Status,
    int? AssignedTechnicianId);
