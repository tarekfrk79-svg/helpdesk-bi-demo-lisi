namespace HelpDeskBiDemo.Application.Common.Notifications;

public sealed record NotificationCenterDto(
    string AudienceTitle,
    string AudienceDescription,
    int UnreadCount,
    int LiveAlertCount,
    IReadOnlyList<NotificationItemDto> Notifications,
    IReadOnlyList<NotificationItemDto> LiveAlerts);
