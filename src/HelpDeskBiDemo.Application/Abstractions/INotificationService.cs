using HelpDeskBiDemo.Application.Common.Notifications;
using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Application.Abstractions;

public interface INotificationService
{
    Task<NotificationHeaderSummaryDto?> GetHeaderSummaryAsync(
        int companyId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default);

    Task<NotificationCenterDto?> GetCenterAsync(
        int companyId,
        DemoRole role,
        int? personId,
        string? activePersonDisplayName,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        int companyId,
        DemoRole role,
        int? personId,
        int notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        int companyId,
        DemoRole role,
        int? personId,
        CancellationToken cancellationToken = default);
}
