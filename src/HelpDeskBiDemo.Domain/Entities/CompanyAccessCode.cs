namespace HelpDeskBiDemo.Domain.Entities;

public sealed class CompanyAccessCode : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company? Company { get; set; }

    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int UsageCount { get; set; }

    public DateTime? LastUsedAtUtc { get; set; }

    public ICollection<CodeUsageLog> UsageLogs { get; set; } = new List<CodeUsageLog>();
}
