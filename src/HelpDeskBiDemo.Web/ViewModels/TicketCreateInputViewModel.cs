using System.ComponentModel.DataAnnotations;
using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Web.ViewModels;

public sealed class TicketCreateInputViewModel
{
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire.")]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TicketCategory Category { get; set; } = TicketCategory.Other;

    [Required]
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
}
