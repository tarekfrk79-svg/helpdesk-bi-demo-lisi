using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Common.Notifications;

public sealed record NotificationItemDto(
    int? NotificationId,
    NotificationType NotificationType,
    string Title,
    string Message,
    string? ActionUrl,
    DateTime CreatedAtUtc,
    bool IsRead,
    bool IsLiveAlert);
