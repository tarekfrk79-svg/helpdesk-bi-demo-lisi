namespace HelpDeskBiDemo.Infrastructure.Options;

public sealed class OwnerOptions
{
    public const string SectionName = "Owner";

    public string AccessCode { get; init; } = string.Empty;
}
