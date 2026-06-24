namespace HelpDeskBiDemo.Application.Common.Owner;

public sealed record OwnerDashboardDto(
    int TotalCompanies,
    int ActiveCompanies,
    int TotalTickets,
    int TotalAccessUsages,
    IReadOnlyList<OwnerCompanySummaryDto> Companies);
