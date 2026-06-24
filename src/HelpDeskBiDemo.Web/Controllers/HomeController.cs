using System.Diagnostics;
using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Access;
using HelpDeskBiDemo.Web.Models;
using HelpDeskBiDemo.Web.Extensions;
using HelpDeskBiDemo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskBiDemo.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly IAccessService _accessService;

    public HomeController(IAccessService accessService)
    {
        _accessService = accessService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new HomeAccessViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterCode(HomeAccessViewModel viewModel, CancellationToken cancellationToken)
    {
        var resolution = await _accessService.ResolveAsync(viewModel.AccessCode, cancellationToken);

        if (resolution.TargetType == AccessTargetType.Invalid)
        {
            ModelState.AddModelError(nameof(HomeAccessViewModel.AccessCode), resolution.ErrorMessage ?? "Code invalide.");
            return View(nameof(Index), viewModel);
        }

        if (resolution.TargetType == AccessTargetType.Owner)
        {
            HttpContext.Session.ClearCompanyContext();
            HttpContext.Session.SetOwnerAuthorized();
            return RedirectToAction("Index", "Owner");
        }

        HttpContext.Session.ClearOwnerAuthorization();
        HttpContext.Session.SetCompanyContext(resolution.CompanyId!.Value, resolution.CompanyName!);
        HttpContext.Session.ClearRoleContext();

        return RedirectToAction("RoleSelection", "Demo");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetAccess()
    {
        HttpContext.Session.ClearOwnerAuthorization();
        HttpContext.Session.ClearCompanyContext();
        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
