namespace HelpDeskBiDemo.Application.Common.Tickets;

public sealed record TicketCommentDto(
    string AuthorDisplayName,
    DateTime CreatedAtUtc,
    string Content);
