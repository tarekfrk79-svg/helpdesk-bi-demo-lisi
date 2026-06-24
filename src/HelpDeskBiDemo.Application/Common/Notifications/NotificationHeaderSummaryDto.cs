namespace HelpDeskBiDemo.Application.Common.Notifications;

public sealed record NotificationHeaderSummaryDto(
    int UnreadCount,
    int LiveAlertCount)
{
    public int AttentionCount => UnreadCount + LiveAlertCount;
}
