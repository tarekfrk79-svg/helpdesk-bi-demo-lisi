using HelpDeskBiDemo.Domain.Enums;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal static class DemoRoleFormatter
{
    public static string ToDisplayLabel(DemoRole role) =>
        role switch
        {
            DemoRole.Owner => "Owner",
            DemoRole.CompanyAdmin => "Admin entreprise",
            DemoRole.SupportTechnician => "Technicien support",
            DemoRole.EndUser => "Utilisateur / Client interne",
            _ => role.ToString()
        };
}
