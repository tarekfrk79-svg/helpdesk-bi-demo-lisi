namespace HelpDeskBiDemo.Application.Common.Access;

public sealed record AccessCodeResolution(
    AccessTargetType TargetType,
    int? CompanyId = null,
    string? CompanyName = null,
    string? ErrorMessage = null);
