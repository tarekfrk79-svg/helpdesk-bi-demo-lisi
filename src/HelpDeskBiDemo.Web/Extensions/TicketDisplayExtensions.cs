using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Web.Extensions;

public static class TicketDisplayExtensions
{
    public static string ToDisplayLabel(this TicketStatus status) =>
        status switch
        {
            TicketStatus.New => "Nouveau",
            TicketStatus.InProgress => "En cours",
            TicketStatus.Resolved => "Resolu",
            TicketStatus.Closed => "Clos",
            _ => status.ToString()
        };

    public static string ToDisplayLabel(this TicketPriority priority) =>
        priority switch
        {
            TicketPriority.Low => "Basse",
            TicketPriority.Normal => "Normale",
            TicketPriority.High => "Haute",
            TicketPriority.Urgent => "Urgente",
            _ => priority.ToString()
        };

    public static string ToDisplayLabel(this TicketCategory category) =>
        category switch
        {
            TicketCategory.Software => "Logiciel",
            TicketCategory.Hardware => "Materiel",
            TicketCategory.Access => "Acces",
            TicketCategory.Bug => "Bug",
            TicketCategory.Other => "Autre",
            _ => category.ToString()
        };

    public static string ToDisplayLabel(this TicketActivityType activityType) =>
        activityType switch
        {
            TicketActivityType.Created => "Creation",
            TicketActivityType.AssignmentChanged => "Affectation",
            TicketActivityType.StatusChanged => "Statut",
            TicketActivityType.CommentAdded => "Commentaire",
            _ => activityType.ToString()
        };

    public static string ToDisplayLabel(this NotificationType notificationType) =>
        notificationType switch
        {
            NotificationType.TicketCreated => "Nouveau ticket",
            NotificationType.TicketAssigned => "Affectation",
            NotificationType.TicketStatusUpdated => "Statut",
            NotificationType.TicketResolved => "Resolution",
            NotificationType.TicketCommentAdded => "Commentaire",
            NotificationType.SlaAlert => "Alerte",
            _ => notificationType.ToString()
        };

    public static string ToStatusCssClass(this TicketStatus status) =>
        status switch
        {
            TicketStatus.New => "status-new",
            TicketStatus.InProgress => "status-progress",
            TicketStatus.Resolved => "status-resolved",
            TicketStatus.Closed => "status-closed",
            _ => "status-closed"
        };

    public static string ToPriorityCssClass(this TicketPriority priority) =>
        priority switch
        {
            TicketPriority.Low => "priority-low",
            TicketPriority.Normal => "priority-normal",
            TicketPriority.High => "priority-high",
            TicketPriority.Urgent => "priority-urgent",
            _ => "priority-normal"
        };

    public static string ToActivityCssClass(this TicketActivityType activityType) =>
        activityType switch
        {
            TicketActivityType.Created => "activity-created",
            TicketActivityType.AssignmentChanged => "activity-assignment",
            TicketActivityType.StatusChanged => "activity-status",
            TicketActivityType.CommentAdded => "activity-comment",
            _ => "activity-status"
        };

    public static string ToNotificationCssClass(this NotificationType notificationType) =>
        notificationType switch
        {
            NotificationType.TicketCreated => "notification-created",
            NotificationType.TicketAssigned => "notification-assignment",
            NotificationType.TicketStatusUpdated => "notification-status",
            NotificationType.TicketResolved => "notification-resolved",
            NotificationType.TicketCommentAdded => "notification-comment",
            NotificationType.SlaAlert => "notification-alert",
            _ => "notification-status"
        };

    public static string ToSlaCssClass(this bool isOverdue)
    {
        return isOverdue ? "sla-overdue" : "sla-on-track";
    }

    public static string FormatHours(this double? value)
    {
        if (!value.HasValue)
        {
            return "N/A";
        }

        var totalHours = value.Value;
        if (totalHours < 24)
        {
            return $"{totalHours:0.#} h";
        }

        var totalDays = Math.Floor(totalHours / 24);
        var remainingHours = totalHours - (totalDays * 24);
        return remainingHours < 0.1
            ? $"{totalDays:0} j"
            : $"{totalDays:0} j {remainingHours:0.#} h";
    }

    public static string FormatHours(this double value)
    {
        return ((double?)value).FormatHours();
    }
}
