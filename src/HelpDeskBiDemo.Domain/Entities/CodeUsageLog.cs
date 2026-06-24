namespace HelpDeskBiDemo.Domain.Entities;

public sealed class CodeUsageLog : Entity
{
    public int CompanyAccessCodeId { get; set; }

    public CompanyAccessCode? CompanyAccessCode { get; set; }

    public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;

    public string Source { get; set; } = "landing-page";
}
