using HelpDeskBiDemo.Application.Common.Tickets;
using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Abstractions;

public interface ITicketService
{
    Task<TicketBoardDto?> GetBoardAsync(
        int companyId,
        DemoRole role,
        int? personId,
        string? searchTerm,
        TicketStatus? statusFilter,
        TicketPriority? priorityFilter,
        TicketCategory? categoryFilter,
        int? assignedTechnicianIdFilter,
        DateTime? createdFromDate,
        DateTime? createdToDate,
        bool onlyMine,
        CancellationToken cancellationToken = default);

    Task<TicketDetailDto?> GetDetailAsync(
        int companyId,
        int ticketId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        int companyId,
        int requesterPersonId,
        CreateTicketCommand command,
        CancellationToken cancellationToken = default);

    Task AddCommentAsync(
        int companyId,
        int ticketId,
        DemoRole role,
        int authorPersonId,
        string content,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        int companyId,
        int ticketId,
        DemoRole role,
        int? actorPersonId,
        ManageTicketCommand command,
        CancellationToken cancellationToken = default);
}
