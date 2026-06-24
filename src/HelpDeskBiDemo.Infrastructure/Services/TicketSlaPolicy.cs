using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal static class TicketSlaPolicy
{
    public static int GetTargetHours(TicketPriority priority) =>
        priority switch
        {
            TicketPriority.Urgent => 4,
            TicketPriority.High => 8,
            TicketPriority.Normal => 24,
            TicketPriority.Low => 48,
            _ => 24
        };

    public static DateTime GetDueAtUtc(TicketPriority priority, DateTime createdAtUtc)
    {
        return createdAtUtc.AddHours(GetTargetHours(priority));
    }

    public static bool IsOverdue(
        TicketPriority priority,
        DateTime createdAtUtc,
        TicketStatus status,
        DateTime utcNow)
    {
        return status != TicketStatus.Resolved &&
               status != TicketStatus.Closed &&
               utcNow > GetDueAtUtc(priority, createdAtUtc);
    }

    public static double? GetTechnicianResolutionHours(DateTime? assignedAtUtc, DateTime? resolvedAtUtc)
    {
        if (!assignedAtUtc.HasValue || !resolvedAtUtc.HasValue)
        {
            return null;
        }

        return Math.Round((resolvedAtUtc.Value - assignedAtUtc.Value).TotalHours, 1);
    }

    public static double? GetTechnicianActiveHours(DateTime? assignedAtUtc, DateTime? resolvedAtUtc, DateTime utcNow)
    {
        if (!assignedAtUtc.HasValue)
        {
            return null;
        }

        var end = resolvedAtUtc ?? utcNow;
        return Math.Round((end - assignedAtUtc.Value).TotalHours, 1);
    }
}
