using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskBiDemo.Web.Controllers;

public sealed class OwnerController : Controller
{
    private readonly IOwnerManagementService _ownerManagementService;

    public OwnerController(IOwnerManagementService ownerManagementService)
    {
        _ownerManagementService = ownerManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsOwnerAuthorized())
        {
            return RedirectToAction("Index", "Home");
        }

        var dashboard = await _ownerManagementService.GetDashboardAsync(cancellationToken);
        return View(dashboard);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(string companyName, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsOwnerAuthorized())
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            await _ownerManagementService.CreateCompanyAsync(companyName, cancellationToken);
            TempData["FlashMessage"] = "Entreprise demo creee avec succes.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["FlashMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompany(int companyId, bool isActive, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsOwnerAuthorized())
        {
            return RedirectToAction("Index", "Home");
        }

        await _ownerManagementService.SetCompanyActivationAsync(companyId, isActive, cancellationToken);
        TempData["FlashMessage"] = isActive
            ? "Entreprise reactivee."
            : "Entreprise desactivee.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetCompany(int companyId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsOwnerAuthorized())
        {
            return RedirectToAction("Index", "Home");
        }

        await _ownerManagementService.ResetCompanyDataAsync(companyId, cancellationToken);
        TempData["FlashMessage"] = "Les tickets de l'entreprise ont ete reinitialises.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCompany(int companyId, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.IsOwnerAuthorized())
        {
            return RedirectToAction("Index", "Home");
        }

        await _ownerManagementService.DeleteCompanyAsync(companyId, cancellationToken);
        TempData["FlashMessage"] = "Entreprise supprimee.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Exit()
    {
        HttpContext.Session.ClearOwnerAuthorization();
        return RedirectToAction("Index", "Home");
    }
}
