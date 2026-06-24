using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Domain.Entities;

public sealed class TicketActivity : Entity
{
    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public TicketActivityType ActivityType { get; set; }

    public int? ActorPersonId { get; set; }

    public DemoPerson? ActorPerson { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
