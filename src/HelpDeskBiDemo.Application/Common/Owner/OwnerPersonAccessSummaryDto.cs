namespace HelpDeskBiDemo.Application.Common.Owner;

public sealed record OwnerPersonAccessSummaryDto(
    int PersonId,
    string FullName,
    string RoleLabel,
    string JobTitle,
    string Department,
    DateTime? LastSignedInAtUtc);
