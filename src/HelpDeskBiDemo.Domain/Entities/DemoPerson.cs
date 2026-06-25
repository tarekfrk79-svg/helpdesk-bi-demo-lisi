using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Domain.Entities;

public sealed class DemoPerson : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company? Company { get; set; }

    public DemoRole Role { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? LastSignedInAtUtc { get; set; }

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
}
