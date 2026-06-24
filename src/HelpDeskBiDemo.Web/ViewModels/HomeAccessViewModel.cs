using System.ComponentModel.DataAnnotations;

namespace HelpDeskBiDemo.Web.ViewModels;

public sealed class HomeAccessViewModel
{
    [Required(ErrorMessage = "Le code d'acces est obligatoire.")]
    public string AccessCode { get; set; } = string.Empty;
}
