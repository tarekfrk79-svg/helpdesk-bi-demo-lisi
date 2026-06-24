using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Web.Extensions;

public static class DemoRoleExtensions
{
    public static string ToDisplayLabel(this DemoRole role) =>
        role switch
        {
            DemoRole.Owner => "Owner",
            DemoRole.CompanyAdmin => "Admin entreprise",
            DemoRole.SupportTechnician => "Technicien support",
            DemoRole.EndUser => "Utilisateur / Client interne",
            _ => role.ToString()
        };
}
