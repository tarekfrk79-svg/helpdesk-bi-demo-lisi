using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Demo;
using HelpDeskBiDemo.Application.Common.Tickets;
using HelpDeskBiDemo.Domain.Entities;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class TicketService : ITicketService
{
    private readonly ApplicationDbContext _dbContext;

    public TicketService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TicketBoardDto?> GetBoardAsync(
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
        CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == companyId && x.IsActive, cancellationToken);

        if (company is null)
        {
            return null;
        }

        var scopedQuery = BuildScopedTicketQuery(companyId, role, personId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmedSearch = searchTerm.Trim();
            scopedQuery = scopedQuery.Where(x =>
                x.Title.Contains(trimmedSearch) ||
                x.Description.Contains(trimmedSearch));
        }

        if (statusFilter.HasValue)
        {
            scopedQuery = scopedQuery.Where(x => x.Status == statusFilter.Value);
        }

        if (priorityFilter.HasValue)
        {
            scopedQuery = scopedQuery.Where(x => x.Priority == priorityFilter.Value);
        }

        if (categoryFilter.HasValue)
        {
            scopedQuery = scopedQuery.Where(x => x.Category == categoryFilter.Value);
        }

        if (assignedTechnicianIdFilter.HasValue)
        {
            scopedQuery = scopedQuery.Where(x => x.AssignedTechnicianId == assignedTechnicianIdFilter.Value);
        }

        if (createdFromDate.HasValue)
        {
            var fromUtc = createdFromDate.Value.Date;
            scopedQuery = scopedQuery.Where(x => x.CreatedAtUtc >= fromUtc);
        }

        if (createdToDate.HasValue)
        {
            var untilExclusiveUtc = createdToDate.Value.Date.AddDays(1);
            scopedQuery = scopedQuery.Where(x => x.CreatedAtUtc < untilExclusiveUtc);
        }

        if (onlyMine && role == DemoRole.SupportTechnician && personId.HasValue)
        {
            scopedQuery = scopedQuery.Where(x => x.AssignedTechnicianId == personId.Value);
        }

        var utcNow = DateTime.UtcNow;

        var rawTickets = await scopedQuery
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(ticket => new
            {
                ticket.Id,
                ticket.Title,
                ticket.Category,
                ticket.Priority,
                ticket.Status,
                RequesterName = ticket.CreatedByPerson!.FullName,
                ticket.AssignedTechnicianId,
                AssignedTechnicianName = ticket.AssignedTechnician != null ? ticket.AssignedTechnician.FullName : null,
                ticket.CreatedAtUtc,
                ticket.UpdatedAtUtc,
                ticket.AssignedAtUtc,
                ticket.ResolvedAtUtc,
                CommentCount = ticket.Comments.Count
            })
            .ToListAsync(cancellationToken);

        var tickets = rawTickets
            .Select(ticket =>
            {
                var dueAtUtc = TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc);
                return new TicketListItemDto(
                    ticket.Id,
                    ticket.Title,
                    ticket.Category,
                    ticket.Priority,
                    ticket.Status,
                    ticket.RequesterName,
                    ticket.AssignedTechnicianId,
                    ticket.AssignedTechnicianName,
                    ticket.CreatedAtUtc,
                    ticket.UpdatedAtUtc,
                    ticket.AssignedAtUtc,
                    dueAtUtc,
                    TicketSlaPolicy.IsOverdue(ticket.Priority, ticket.CreatedAtUtc, ticket.Status, utcNow),
                    TicketSlaPolicy.GetTechnicianResolutionHours(ticket.AssignedAtUtc, ticket.ResolvedAtUtc),
                    ticket.CommentCount);
            })
            .ToList();

        var technicians = await GetTechnicianOptionsAsync(companyId, cancellationToken);
        var activePerson = await GetActivePersonDisplayNameAsync(companyId, personId, cancellationToken);

        return new TicketBoardDto(
            company.Id,
            company.Name,
            role,
            DemoRoleFormatter.ToDisplayLabel(role),
            activePerson,
            searchTerm,
            statusFilter,
            priorityFilter,
            categoryFilter,
            assignedTechnicianIdFilter,
            createdFromDate,
            createdToDate,
            onlyMine,
            role == DemoRole.EndUser,
            role == DemoRole.CompanyAdmin || role == DemoRole.SupportTechnician,
            technicians,
            tickets);
    }

    public async Task<TicketDetailDto?> GetDetailAsync(
        int companyId,
        int ticketId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await BuildScopedTicketQuery(companyId, role, personId)
            .Where(x => x.Id == ticketId)
            .Select(x => new
            {
                x.Id,
                x.CompanyId,
                CompanyName = x.Company!.Name,
                x.Title,
                x.Description,
                x.Category,
                x.Priority,
                x.Status,
                RequesterName = x.CreatedByPerson!.FullName,
                x.AssignedTechnicianId,
                AssignedTechnicianName = x.AssignedTechnician != null ? x.AssignedTechnician.FullName : null,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.AssignedAtUtc,
                x.ResolvedAtUtc,
                Activities = x.Activities
                    .OrderByDescending(activity => activity.CreatedAtUtc)
                    .Select(activity => new TicketActivityDto(
                        activity.ActivityType,
                        activity.Description,
                        activity.ActorPerson == null
                            ? null
                            : string.IsNullOrWhiteSpace(activity.ActorPerson.Department)
                                ? $"{activity.ActorPerson.FullName} - {activity.ActorPerson.JobTitle}"
                                : $"{activity.ActorPerson.FullName} - {activity.ActorPerson.Department}",
                        activity.CreatedAtUtc))
                    .ToList(),
                Comments = x.Comments
                    .OrderBy(comment => comment.CreatedAtUtc)
                    .Select(comment => new TicketCommentDto(
                        string.IsNullOrWhiteSpace(comment.AuthorPerson!.Department)
                            ? $"{comment.AuthorPerson.FullName} - {comment.AuthorPerson.JobTitle}"
                            : $"{comment.AuthorPerson.FullName} - {comment.AuthorPerson.Department}",
                        comment.CreatedAtUtc,
                        comment.Content))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        var technicians = await GetTechnicianOptionsAsync(companyId, cancellationToken);
        var activePerson = await GetActivePersonDisplayNameAsync(companyId, personId, cancellationToken);
        var dueAtUtc = TicketSlaPolicy.GetDueAtUtc(ticket.Priority, ticket.CreatedAtUtc);
        var utcNow = DateTime.UtcNow;

        return new TicketDetailDto(
            ticket.Id,
            ticket.CompanyId,
            ticket.CompanyName,
            role,
            DemoRoleFormatter.ToDisplayLabel(role),
            activePerson,
            ticket.Title,
            ticket.Description,
            ticket.Category,
            ticket.Priority,
            ticket.Status,
            ticket.RequesterName,
            ticket.AssignedTechnicianId,
            ticket.AssignedTechnicianName,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.AssignedAtUtc,
            ticket.ResolvedAtUtc,
            dueAtUtc,
            TicketSlaPolicy.IsOverdue(ticket.Priority, ticket.CreatedAtUtc, ticket.Status, utcNow),
            TicketSlaPolicy.GetTargetHours(ticket.Priority),
            TicketSlaPolicy.GetTechnicianResolutionHours(ticket.AssignedAtUtc, ticket.ResolvedAtUtc),
            TicketSlaPolicy.GetTechnicianActiveHours(ticket.AssignedAtUtc, ticket.ResolvedAtUtc, utcNow),
            role == DemoRole.CompanyAdmin || role == DemoRole.SupportTechnician,
            true,
            role == DemoRole.SupportTechnician && !ticket.AssignedTechnicianId.HasValue,
            technicians,
            ticket.Activities,
            ticket.Comments);
    }

    public async Task<int> CreateAsync(
        int companyId,
        int requesterPersonId,
        CreateTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title) || string.IsNullOrWhiteSpace(command.Description))
        {
            throw new InvalidOperationException("Le titre et la description sont obligatoires.");
        }

        var requester = await _dbContext.DemoPeople
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.CompanyId == companyId &&
                     x.Id == requesterPersonId &&
                     x.Role == DemoRole.EndUser &&
                     x.IsActive,
                cancellationToken);

        if (requester is null)
        {
            throw new InvalidOperationException("Utilisateur demo introuvable.");
        }

        var now = DateTime.UtcNow;
        var ticket = new Ticket
        {
            CompanyId = companyId,
            CreatedByPersonId = requesterPersonId,
            Title = command.Title.Trim(),
            Description = command.Description.Trim(),
            Category = command.Category,
            Priority = command.Priority,
            Status = TicketStatus.New,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        ticket.Activities.Add(new TicketActivity
        {
            ActorPersonId = requesterPersonId,
            ActivityType = TicketActivityType.Created,
            Description = BuildCreatedDescription(command.Category, command.Priority),
            CreatedAtUtc = now
        });

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        QueueNotification(
            companyId,
            ticket.Id,
            NotificationType.TicketCreated,
            DemoRole.CompanyAdmin,
            recipientPersonId: null,
            title: "Nouveau ticket utilisateur",
            message: $"{requester.FullName} a cree le ticket \"{ticket.Title}\" avec une priorite {TicketLabelFormatter.ToDisplayLabel(ticket.Priority).ToLowerInvariant()}.");

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ticket.Id;
    }

    public async Task AddCommentAsync(
        int companyId,
        int ticketId,
        DemoRole role,
        int authorPersonId,
        string content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Le commentaire ne peut pas etre vide.");
        }

        var ticket = await BuildScopedTicketQuery(companyId, role, authorPersonId)
            .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            throw new InvalidOperationException("Ticket introuvable.");
        }

        var author = await _dbContext.DemoPeople
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == authorPersonId && x.IsActive, cancellationToken);

        if (author is null)
        {
            throw new InvalidOperationException("Auteur demo introuvable.");
        }

        var now = DateTime.UtcNow;
        ticket.Comments.Add(new TicketComment
        {
            AuthorPersonId = authorPersonId,
            Content = content.Trim(),
            CreatedAtUtc = now
        });

        ticket.Activities.Add(new TicketActivity
        {
            ActorPersonId = authorPersonId,
            ActivityType = TicketActivityType.CommentAdded,
            Description = "Commentaire ajoute sur le ticket.",
            CreatedAtUtc = now
        });

        if (ticket.CreatedByPersonId != authorPersonId)
        {
            QueueNotification(
                companyId,
                ticket.Id,
                NotificationType.TicketCommentAdded,
                DemoRole.EndUser,
                ticket.CreatedByPersonId,
                "Nouveau commentaire sur ton ticket",
                $"{author.FullName} a repondu sur le ticket \"{ticket.Title}\".");
        }

        if (ticket.AssignedTechnicianId.HasValue && ticket.AssignedTechnicianId.Value != authorPersonId)
        {
            QueueNotification(
                companyId,
                ticket.Id,
                NotificationType.TicketCommentAdded,
                DemoRole.SupportTechnician,
                ticket.AssignedTechnicianId.Value,
                role == DemoRole.EndUser ? "Reponse utilisateur recue" : "Mise a jour sur un ticket assigne",
                $"{author.FullName} a ajoute un commentaire sur le ticket \"{ticket.Title}\".");
        }

        if (role == DemoRole.EndUser)
        {
            QueueNotification(
                companyId,
                ticket.Id,
                NotificationType.TicketCommentAdded,
                DemoRole.CompanyAdmin,
                recipientPersonId: null,
                "Utilisateur en attente de reponse",
                $"{author.FullName} a ajoute un commentaire sur le ticket \"{ticket.Title}\".");
        }

        ticket.UpdatedAtUtc = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        int companyId,
        int ticketId,
        DemoRole role,
        int? actorPersonId,
        ManageTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        if (role != DemoRole.CompanyAdmin && role != DemoRole.SupportTechnician)
        {
            throw new InvalidOperationException("Cette action n'est pas autorisee.");
        }

        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            throw new InvalidOperationException("Ticket introuvable.");
        }

        string? actorDisplayName = null;
        string? oldAssignedName = null;
        string? newAssignedName = null;

        if (ticket.AssignedTechnicianId.HasValue)
        {
            oldAssignedName = await GetPersonNameAsync(companyId, ticket.AssignedTechnicianId.Value, cancellationToken);
        }

        var oldStatus = ticket.Status;
        var oldAssignedTechnicianId = ticket.AssignedTechnicianId;
        var newAssignedTechnicianId = command.AssignedTechnicianId;
        var now = DateTime.UtcNow;
        var requesterPersonId = ticket.CreatedByPersonId;

        if (role == DemoRole.SupportTechnician)
        {
            if (!actorPersonId.HasValue)
            {
                throw new InvalidOperationException("Technicien demo introuvable.");
            }

            var actor = await _dbContext.DemoPeople
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CompanyId == companyId &&
                         x.Id == actorPersonId.Value &&
                         x.Role == DemoRole.SupportTechnician &&
                         x.IsActive,
                    cancellationToken);

            if (actor is null)
            {
                throw new InvalidOperationException("Technicien demo introuvable.");
            }

            actorDisplayName = actor.FullName;

            if (ticket.AssignedTechnicianId.HasValue && ticket.AssignedTechnicianId != actorPersonId.Value)
            {
                throw new InvalidOperationException("Ce ticket est deja pris par un autre technicien.");
            }

            if (command.Status == TicketStatus.New)
            {
                throw new InvalidOperationException("Un technicien ne peut pas remettre un ticket en statut Nouveau.");
            }

            newAssignedTechnicianId = actorPersonId.Value;
            newAssignedName = actor.FullName;
        }
        else
        {
            if (actorPersonId.HasValue)
            {
                actorDisplayName = await GetPersonNameAsync(companyId, actorPersonId.Value, cancellationToken);
            }

            if (command.AssignedTechnicianId.HasValue)
            {
                var technician = await _dbContext.DemoPeople
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.CompanyId == companyId &&
                             x.Id == command.AssignedTechnicianId.Value &&
                             x.Role == DemoRole.SupportTechnician &&
                             x.IsActive,
                        cancellationToken);

                if (technician is null)
                {
                    throw new InvalidOperationException("Technicien assigne invalide.");
                }

                newAssignedName = technician.FullName;
            }
        }

        if (newAssignedTechnicianId == oldAssignedTechnicianId && !string.IsNullOrWhiteSpace(oldAssignedName))
        {
            newAssignedName ??= oldAssignedName;
        }

        var assignmentChanged = oldAssignedTechnicianId != newAssignedTechnicianId;
        var statusChanged = oldStatus != command.Status;

        if (!assignmentChanged && !statusChanged)
        {
            throw new InvalidOperationException("Aucune modification detectee.");
        }

        if (assignmentChanged)
        {
            ticket.AssignedTechnicianId = newAssignedTechnicianId;
            ticket.AssignedAtUtc = newAssignedTechnicianId.HasValue ? now : null;
        }

        if (statusChanged)
        {
            ticket.Status = command.Status;

            if (command.Status == TicketStatus.Resolved)
            {
                ticket.ResolvedAtUtc = now;
            }
            else if (command.Status == TicketStatus.New || command.Status == TicketStatus.InProgress)
            {
                ticket.ResolvedAtUtc = null;
            }
        }

        ticket.UpdatedAtUtc = now;

        if (assignmentChanged)
        {
            ticket.Activities.Add(new TicketActivity
            {
                ActorPersonId = actorPersonId,
                ActivityType = TicketActivityType.AssignmentChanged,
                Description = BuildAssignmentDescription(oldAssignedName, newAssignedName),
                CreatedAtUtc = now
            });

            if (oldAssignedTechnicianId.HasValue &&
                oldAssignedTechnicianId != actorPersonId &&
                oldAssignedTechnicianId != newAssignedTechnicianId)
            {
                QueueNotification(
                    companyId,
                    ticket.Id,
                    NotificationType.TicketAssigned,
                    DemoRole.SupportTechnician,
                    oldAssignedTechnicianId.Value,
                    "Ticket reaffecte",
                    $"Le ticket \"{ticket.Title}\" n'est plus dans ta file de traitement.");
            }

            if (newAssignedTechnicianId.HasValue && newAssignedTechnicianId != actorPersonId)
            {
                QueueNotification(
                    companyId,
                    ticket.Id,
                    NotificationType.TicketAssigned,
                    DemoRole.SupportTechnician,
                    newAssignedTechnicianId.Value,
                    oldAssignedTechnicianId.HasValue ? "Ticket reaffecte sur ton profil" : "Nouveau ticket assigne",
                    $"{(string.IsNullOrWhiteSpace(actorDisplayName) ? "Un responsable" : actorDisplayName)} t'a confie le ticket \"{ticket.Title}\".");
            }

            if (newAssignedTechnicianId.HasValue && requesterPersonId != actorPersonId)
            {
                QueueNotification(
                    companyId,
                    ticket.Id,
                    NotificationType.TicketAssigned,
                    DemoRole.EndUser,
                    requesterPersonId,
                    oldAssignedTechnicianId.HasValue ? "Ton ticket a ete reaffecte" : "Ton ticket est pris en charge",
                    $"{newAssignedName} s'occupe maintenant du ticket \"{ticket.Title}\".");
            }

            if (role == DemoRole.SupportTechnician && newAssignedTechnicianId.HasValue)
            {
                QueueNotification(
                    companyId,
                    ticket.Id,
                    NotificationType.TicketAssigned,
                    DemoRole.CompanyAdmin,
                    recipientPersonId: null,
                    "Ticket pris en charge",
                    $"{newAssignedName} a pris en charge le ticket \"{ticket.Title}\".");
            }
        }

        if (statusChanged)
        {
            ticket.Activities.Add(new TicketActivity
            {
                ActorPersonId = actorPersonId,
                ActivityType = TicketActivityType.StatusChanged,
                Description = BuildStatusDescription(oldStatus, command.Status, actorDisplayName),
                CreatedAtUtc = now
            });

            var skipRequesterStatusNotification =
                assignmentChanged &&
                oldStatus == TicketStatus.New &&
                command.Status == TicketStatus.InProgress;

            if (!skipRequesterStatusNotification && requesterPersonId != actorPersonId)
            {
                QueueNotification(
                    companyId,
                    ticket.Id,
                    command.Status == TicketStatus.Resolved || command.Status == TicketStatus.Closed
                        ? NotificationType.TicketResolved
                        : NotificationType.TicketStatusUpdated,
                    DemoRole.EndUser,
                    requesterPersonId,
                    command.Status == TicketStatus.Resolved
                        ? "Ton ticket est resolu"
                        : command.Status == TicketStatus.Closed
                            ? "Ton ticket est clos"
                            : "Le statut de ton ticket a change",
                    $"Le ticket \"{ticket.Title}\" est maintenant au statut {TicketLabelFormatter.ToDisplayLabel(command.Status).ToLowerInvariant()}.");
            }

            if ((command.Status == TicketStatus.Resolved || command.Status == TicketStatus.Closed) && role == DemoRole.SupportTechnician)
            {
                QueueNotification(
                    companyId,
                    ticket.Id,
                    NotificationType.TicketResolved,
                    DemoRole.CompanyAdmin,
                    recipientPersonId: null,
                    command.Status == TicketStatus.Resolved ? "Ticket resolu" : "Ticket clos",
                    $"{(string.IsNullOrWhiteSpace(actorDisplayName) ? "Un technicien" : actorDisplayName)} a passe \"{ticket.Title}\" au statut {TicketLabelFormatter.ToDisplayLabel(command.Status).ToLowerInvariant()}.");
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Ticket> BuildScopedTicketQuery(int companyId, DemoRole role, int? personId)
    {
        var query = _dbContext.Tickets
            .Where(x => x.CompanyId == companyId)
            .AsQueryable();

        if (role == DemoRole.SupportTechnician && !personId.HasValue)
        {
            query = query.Where(_ => false);
        }

        if (role == DemoRole.SupportTechnician && personId.HasValue)
        {
            query = query.Where(x => x.AssignedTechnicianId == personId.Value || x.AssignedTechnicianId == null);
        }

        if (role == DemoRole.EndUser && !personId.HasValue)
        {
            query = query.Where(_ => false);
        }

        if (role == DemoRole.EndUser && personId.HasValue)
        {
            query = query.Where(x => x.CreatedByPersonId == personId.Value);
        }

        return query;
    }

    private async Task<IReadOnlyList<DemoPersonDto>> GetTechnicianOptionsAsync(
        int companyId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Role == DemoRole.SupportTechnician && x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(person => new DemoPersonDto(
                person.Id,
                person.FullName,
                person.JobTitle,
                person.Department,
                person.Role,
                string.IsNullOrWhiteSpace(person.Department)
                    ? $"{person.FullName} - {person.JobTitle}"
                    : $"{person.FullName} - {person.Department}"))
            .ToListAsync(cancellationToken);
    }

    private async Task<string?> GetActivePersonDisplayNameAsync(
        int companyId,
        int? personId,
        CancellationToken cancellationToken)
    {
        if (!personId.HasValue)
        {
            return null;
        }

        return await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Id == personId.Value)
            .Select(person => string.IsNullOrWhiteSpace(person.Department)
                ? $"{person.FullName} - {person.JobTitle}"
                : $"{person.FullName} - {person.Department}")
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<string?> GetPersonNameAsync(
        int companyId,
        int personId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Id == personId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string BuildCreatedDescription(TicketCategory category, TicketPriority priority)
    {
        return $"Ticket cree avec la categorie {TicketLabelFormatter.ToDisplayLabel(category)} et la priorite {TicketLabelFormatter.ToDisplayLabel(priority)}.";
    }

    private static string BuildAssignmentDescription(string? oldAssignedName, string? newAssignedName)
    {
        if (string.IsNullOrWhiteSpace(oldAssignedName) && !string.IsNullOrWhiteSpace(newAssignedName))
        {
            return $"Ticket assigne a {newAssignedName}.";
        }

        if (!string.IsNullOrWhiteSpace(oldAssignedName) && string.IsNullOrWhiteSpace(newAssignedName))
        {
            return $"Affectation retiree (precedemment {oldAssignedName}).";
        }

        return $"Ticket reaffecte de {oldAssignedName} a {newAssignedName}.";
    }

    private static string BuildStatusDescription(
        TicketStatus previousStatus,
        TicketStatus nextStatus,
        string? actorDisplayName)
    {
        var baseDescription =
            $"Statut passe de {TicketLabelFormatter.ToDisplayLabel(previousStatus)} a {TicketLabelFormatter.ToDisplayLabel(nextStatus)}.";

        if (string.IsNullOrWhiteSpace(actorDisplayName))
        {
            return baseDescription;
        }

        return $"{baseDescription} Action effectuee par {actorDisplayName}.";
    }

    private void QueueNotification(
        int companyId,
        int ticketId,
        NotificationType notificationType,
        DemoRole recipientRole,
        int? recipientPersonId,
        string title,
        string message)
    {
        _dbContext.Notifications.Add(new Notification
        {
            CompanyId = companyId,
            TicketId = ticketId,
            NotificationType = notificationType,
            RecipientRole = recipientRole,
            RecipientPersonId = recipientPersonId,
            Title = title,
            Message = message,
            ActionUrl = $"/Tickets/Details/{ticketId}",
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
