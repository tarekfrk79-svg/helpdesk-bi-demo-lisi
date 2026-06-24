using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal static class TicketLabelFormatter
{
    public static string ToDisplayLabel(TicketStatus status) =>
        status switch
        {
            TicketStatus.New => "Nouveau",
            TicketStatus.InProgress => "En cours",
            TicketStatus.Resolved => "Resolu",
            TicketStatus.Closed => "Clos",
            _ => status.ToString()
        };

    public static string ToDisplayLabel(TicketPriority priority) =>
        priority switch
        {
            TicketPriority.Low => "Basse",
            TicketPriority.Normal => "Normale",
            TicketPriority.High => "Haute",
            TicketPriority.Urgent => "Urgente",
            _ => priority.ToString()
        };

    public static string ToDisplayLabel(TicketCategory category) =>
        category switch
        {
            TicketCategory.Software => "Logiciel",
            TicketCategory.Hardware => "Materiel",
            TicketCategory.Access => "Acces",
            TicketCategory.Bug => "Bug",
            TicketCategory.Other => "Autre",
            _ => category.ToString()
        };

    public static string ToDisplayLabel(TicketActivityType activityType) =>
        activityType switch
        {
            TicketActivityType.Created => "Creation",
            TicketActivityType.AssignmentChanged => "Affectation",
            TicketActivityType.StatusChanged => "Statut",
            TicketActivityType.CommentAdded => "Commentaire",
            _ => activityType.ToString()
        };
}
