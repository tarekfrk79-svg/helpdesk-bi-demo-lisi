namespace HelpDeskBiDemo.Application.Common.Owner;

public sealed record OwnerCompanySummaryDto(
    int CompanyId,
    string CompanyName,
    string CompanySlug,
    string AccessCode,
    bool IsActive,
    int UsageCount,
    DateTime? LastUsedAtUtc,
    int TicketCount,
    int PersonCount,
    DateTime CreatedAtUtc,
    DateTime? LastResetAtUtc,
    IReadOnlyList<OwnerPersonAccessSummaryDto> DemoAccounts);
