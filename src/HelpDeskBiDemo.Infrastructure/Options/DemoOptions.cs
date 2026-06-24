namespace HelpDeskBiDemo.Infrastructure.Options;

public sealed class DemoOptions
{
    public const string SectionName = "Demo";

    public bool SeedData { get; init; } = true;
}
