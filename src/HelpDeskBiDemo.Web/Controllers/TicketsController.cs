using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Tickets;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Web.Extensions;
using HelpDeskBiDemo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskBiDemo.Web.Controllers;

public sealed class TicketsController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly INotificationService _notificationService;

    public TicketsController(ITicketService ticketService, INotificationService notificationService)
    {
        _ticketService = ticketService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        TicketStatus? statusFilter,
        TicketPriority? priorityFilter,
        TicketCategory? categoryFilter,
        int? assignedTechnicianIdFilter,
        DateTime? createdFromDate,
        DateTime? createdToDate,
        bool onlyMine,
        CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();

        if (!companyId.HasValue || !role.HasValue)
        {
            return RedirectToAction("RoleSelection", "Demo");
        }

        var board = await _ticketService.GetBoardAsync(
            companyId.Value,
            role.Value,
            HttpContext.Session.GetPersonId(),
            searchTerm,
            statusFilter,
            priorityFilter,
            categoryFilter,
            assignedTechnicianIdFilter,
            createdFromDate,
            createdToDate,
            onlyMine,
            cancellationToken);

        if (board is null)
        {
            HttpContext.Session.ClearCompanyContext();
            return RedirectToAction("Index", "Home");
        }

        ViewBag.CreateModel = new TicketCreateInputViewModel();
        return View(board);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateInputViewModel input, CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();
        var personId = HttpContext.Session.GetPersonId();

        if (!companyId.HasValue || !role.HasValue || !personId.HasValue || role.Value != DemoRole.EndUser)
        {
            return RedirectToAction("RoleSelection", "Demo");
        }

        if (!ModelState.IsValid)
        {
            TempData["FlashMessage"] = "Merci de verifier les champs du ticket.";
            return RedirectToAction(nameof(Index));
        }

        var ticketId = await _ticketService.CreateAsync(
            companyId.Value,
            personId.Value,
            new CreateTicketCommand(input.Title, input.Description, input.Category, input.Priority),
            cancellationToken);

        TempData["FlashMessage"] = "Ticket cree avec succes.";
        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, int? notificationId, CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();

        if (!companyId.HasValue || !role.HasValue)
        {
            return RedirectToAction("RoleSelection", "Demo");
        }

        if (notificationId.HasValue)
        {
            // Marks the persistent notification as read when the user opens the related ticket.
            await _notificationService.MarkAsReadAsync(
                companyId.Value,
                role.Value,
                HttpContext.Session.GetPersonId(),
                notificationId.Value,
                cancellationToken);
        }

        var ticket = await _ticketService.GetDetailAsync(
            companyId.Value,
            id,
            role.Value,
            HttpContext.Session.GetPersonId(),
            cancellationToken);

        if (ticket is null)
        {
            TempData["FlashMessage"] = "Ticket introuvable.";
            return RedirectToAction(nameof(Index));
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string content, CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();
        var personId = HttpContext.Session.GetPersonId();

        if (!companyId.HasValue || !role.HasValue || !personId.HasValue)
        {
            return RedirectToAction("RoleSelection", "Demo");
        }

        try
        {
            await _ticketService.AddCommentAsync(
                companyId.Value,
                id,
                role.Value,
                personId.Value,
                content,
                cancellationToken);

            TempData["FlashMessage"] = "Commentaire ajoute.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["FlashMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int id,
        TicketStatus status,
        int? assignedTechnicianId,
        CancellationToken cancellationToken)
    {
        var companyId = HttpContext.Session.GetCompanyId();
        var role = HttpContext.Session.GetRole();

        if (!companyId.HasValue || !role.HasValue)
        {
            return RedirectToAction("RoleSelection", "Demo");
        }

        try
        {
            await _ticketService.UpdateAsync(
                companyId.Value,
                id,
                role.Value,
                HttpContext.Session.GetPersonId(),
                new ManageTicketCommand(status, assignedTechnicianId),
                cancellationToken);

            TempData["FlashMessage"] = "Ticket mis a jour.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["FlashMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
