namespace HelpDeskBiDemo.Domain.Entities;

public sealed class TicketComment : Entity
{
    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public int AuthorPersonId { get; set; }

    public DemoPerson? AuthorPerson { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
