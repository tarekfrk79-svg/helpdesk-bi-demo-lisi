using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Domain.Entities;

public sealed class Notification : Entity
{
    public int CompanyId { get; set; }

    public Company? Company { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public NotificationType NotificationType { get; set; }

    public DemoRole RecipientRole { get; set; }

    public int? RecipientPersonId { get; set; }

    public DemoPerson? RecipientPerson { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string ActionUrl { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAtUtc { get; set; }
}
