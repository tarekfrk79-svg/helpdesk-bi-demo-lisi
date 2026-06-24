namespace HelpDeskBiDemo.Domain.Entities;

public sealed class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? LastResetAtUtc { get; set; }

    public ICollection<CompanyAccessCode> AccessCodes { get; set; } = new List<CompanyAccessCode>();

    public ICollection<DemoPerson> People { get; set; } = new List<DemoPerson>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
