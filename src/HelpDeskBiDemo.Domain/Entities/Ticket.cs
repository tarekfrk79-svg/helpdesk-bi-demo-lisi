using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Domain.Entities;

public sealed class Ticket : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company? Company { get; set; }

    public int CreatedByPersonId { get; set; }

    public DemoPerson? CreatedByPerson { get; set; }

    public int? AssignedTechnicianId { get; set; }

    public DemoPerson? AssignedTechnician { get; set; }

    public DateTime? AssignedAtUtc { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketCategory Category { get; set; }

    public TicketPriority Priority { get; set; } = TicketPriority.Normal;

    public TicketStatus Status { get; set; } = TicketStatus.New;

    public DateTime? ResolvedAtUtc { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

    public ICollection<TicketActivity> Activities { get; set; } = new List<TicketActivity>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
