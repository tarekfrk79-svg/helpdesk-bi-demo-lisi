using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Web.Extensions;
using HelpDeskBiDemo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskBiDemo.Web.Controllers;

public sealed class DemoController : Controller
{
    private readonly IDemoCompanyService _demoCompanyService;

    public DemoController(IDemoCompanyService demoCompanyService)
    {
        _demoCompanyService = demoCompanyService;
    }

    [HttpGet]
    public async Task<IActionResult> RoleSelection(CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        if (!companyId.HasValue)
        {
            return RedirectToAction("Index", "Home");
        }

        var company = await _demoCompanyService.GetCompanyContextAsync(companyId.Value, cancellationToken);
        if (company is null)
        {
            HttpContext.Session.ClearCompanyContext();
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new RoleSelectionViewModel
        {
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectRole(DemoRole role, CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var companyName = HttpContext.Session.GetCompanyName();

        if (!companyId.HasValue || string.IsNullOrWhiteSpace(companyName))
        {
            return RedirectToAction("Index", "Home");
        }

        if (role == DemoRole.CompanyAdmin)
        {
            var admin = await _demoCompanyService.GetDefaultAdminAsync(companyId.Value, cancellationToken);
            if (admin is null)
            {
                TempData["FlashMessage"] = "Aucun administrateur demo n'est disponible.";
                return RedirectToAction(nameof(RoleSelection));
            }

            await _demoCompanyService.MarkPersonLastAccessAsync(
                companyId.Value,
                admin.Id,
                role,
                cancellationToken);

            HttpContext.Session.SetRoleContext(role, admin.Id, admin.DisplayLabel);
            return RedirectToAction(nameof(Dashboard));
        }

        return RedirectToAction(nameof(PersonSelection), new { role });
    }

    [HttpGet]
    public async Task<IActionResult> PersonSelection(DemoRole role, CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var companyName = HttpContext.Session.GetCompanyName();

        if (!companyId.HasValue || string.IsNullOrWhiteSpace(companyName))
        {
            return RedirectToAction("Index", "Home");
        }

        if (role != DemoRole.SupportTechnician && role != DemoRole.EndUser)
        {
            return RedirectToAction(nameof(RoleSelection));
        }

        var people = await _demoCompanyService.GetPeopleByRoleAsync(companyId.Value, role, cancellationToken);

        var viewModel = new PersonSelectionViewModel
        {
            CompanyId = companyId.Value,
            CompanyName = companyName,
            Role = role,
            RoleLabel = role.ToDisplayLabel(),
            People = people
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectPerson(DemoRole role, int personId, CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();

        if (!companyId.HasValue)
        {
            return RedirectToAction("Index", "Home");
        }

        var person = await _demoCompanyService.GetPersonAsync(companyId.Value, personId, role, cancellationToken);
        if (person is null)
        {
            TempData["FlashMessage"] = "Personnage demo introuvable.";
            return RedirectToAction(nameof(PersonSelection), new { role });
        }

        await _demoCompanyService.MarkPersonLastAccessAsync(
            companyId.Value,
            person.Id,
            role,
            cancellationToken);

        HttpContext.Session.SetRoleContext(role, person.Id, person.DisplayLabel);
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();

        if (!companyId.HasValue || !role.HasValue)
        {
            return RedirectToAction(nameof(RoleSelection));
        }

        var dashboard = await _demoCompanyService.GetDashboardAsync(
            companyId.Value,
            role.Value,
            HttpContext.Session.GetPersonId(),
            cancellationToken);

        if (dashboard is null)
        {
            HttpContext.Session.ClearCompanyContext();
            return RedirectToAction("Index", "Home");
        }

        return View(dashboard);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExitCompany()
    {
        HttpContext.Session.ClearCompanyContext();
        return RedirectToAction("Index", "Home");
    }
}
