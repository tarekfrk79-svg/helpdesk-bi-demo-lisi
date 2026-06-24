using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Tickets;

public sealed record TicketActivityDto(
    TicketActivityType ActivityType,
    string Description,
    string? ActorDisplayName,
    DateTime CreatedAtUtc);
