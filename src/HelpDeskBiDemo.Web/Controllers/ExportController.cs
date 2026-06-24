using System.Text;
using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskBiDemo.Web.Controllers;

public sealed class ExportController : Controller
{
    private readonly ICsvExportService _csvExportService;

    public ExportController(ICsvExportService csvExportService)
    {
        _csvExportService = csvExportService;
    }

    [HttpGet]
    public async Task<IActionResult> CompanyCsv(CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();

        if (!companyId.HasValue || !role.HasValue)
        {
            return RedirectToAction("Index", "Home");
        }

        if (role.Value != DemoRole.CompanyAdmin)
        {
            TempData["FlashMessage"] = "L'export CSV complet est reserve a la vue Admin entreprise.";
            return RedirectToAction("Dashboard", "Demo");
        }

        var export = await _csvExportService.ExportCompanyTicketsAsync(companyId.Value, cancellationToken);
        if (export is null)
        {
            TempData["FlashMessage"] = "Impossible de generer l'export pour cette entreprise.";
            return RedirectToAction("Dashboard", "Demo");
        }

        return File(Encoding.UTF8.GetBytes(export.Content), "text/csv; charset=utf-8", export.FileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OwnerCompanyCsv(int companyId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsOwnerAuthorized())
        {
            return RedirectToAction("Index", "Home");
        }

        var export = await _csvExportService.ExportCompanyTicketsAsync(companyId, cancellationToken);
        if (export is null)
        {
            TempData["FlashMessage"] = "Entreprise introuvable pour l'export CSV.";
            return RedirectToAction("Index", "Owner");
        }

        return File(Encoding.UTF8.GetBytes(export.Content), "text/csv; charset=utf-8", export.FileName);
    }
}
