using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskBiDemo.Web.Controllers;

public sealed class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();

        if (!companyId.HasValue || !role.HasValue)
        {
            return RedirectToAction("RoleSelection", "Demo");
        }

        var center = await _notificationService.GetCenterAsync(
            companyId.Value,
            role.Value,
            HttpContext.Session.GetPersonId(),
            HttpContext.Session.GetPersonDisplayName(),
            cancellationToken);

        if (center is null)
        {
            HttpContext.Session.ClearCompanyContext();
            return RedirectToAction("Index", "Home");
        }

        return View(center);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();

        if (!companyId.HasValue || !role.HasValue)
        {
            return RedirectToAction("RoleSelection", "Demo");
        }

        await _notificationService.MarkAllAsReadAsync(
            companyId.Value,
            role.Value,
            HttpContext.Session.GetPersonId(),
            cancellationToken);

        TempData["FlashMessage"] = "Notifications marquees comme lues.";
        return RedirectToAction(nameof(Index));
    }
}
