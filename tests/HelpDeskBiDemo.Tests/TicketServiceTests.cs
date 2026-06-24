using HelpDeskBiDemo.Application.Common.Tickets;
using HelpDeskBiDemo.Domain.Entities;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using HelpDeskBiDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpDeskBiDemo.Tests;

public sealed class TicketServiceTests
{
    [Fact]
    public async Task UpdateAsync_AssignsUnassignedTicketToTechnician()
    {
        await using var dbContext = CreateDbContext();
        var seededData = await SeedScenarioAsync(dbContext, assignedTechnicianId: null, status: TicketStatus.New);
        var service = new TicketService(dbContext);

        await service.UpdateAsync(
            seededData.CompanyId,
            seededData.TicketId,
            DemoRole.SupportTechnician,
            seededData.TechAId,
            new ManageTicketCommand(TicketStatus.InProgress, null));

        var ticket = await dbContext.Tickets.SingleAsync();
        var activities = await dbContext.TicketActivities
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();

        Assert.Equal(seededData.TechAId, ticket.AssignedTechnicianId);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.NotNull(ticket.AssignedAtUtc);
        Assert.Contains(activities, activity => activity.ActivityType == TicketActivityType.AssignmentChanged);
        Assert.Contains(activities, activity => activity.ActivityType == TicketActivityType.StatusChanged);
    }

    [Fact]
    public async Task UpdateAsync_RejectsTakingTicketOwnedByAnotherTechnician()
    {
        await using var dbContext = CreateDbContext();
        var seededData = await SeedScenarioAsync(dbContext, assignedTechnicianId: 3, status: TicketStatus.InProgress);
        var service = new TicketService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(
                seededData.CompanyId,
                seededData.TicketId,
                DemoRole.SupportTechnician,
                seededData.TechAId,
                new ManageTicketCommand(TicketStatus.Resolved, null)));

        Assert.Equal("Ce ticket est deja pris par un autre technicien.", exception.Message);
    }

    [Fact]
    public async Task GetBoardAsync_ForTechnician_ShowsOwnAndUnassignedTicketsOnly()
    {
        await using var dbContext = CreateDbContext();
        var seededData = await SeedScenarioAsync(dbContext, assignedTechnicianId: null, status: TicketStatus.New);

        dbContext.Tickets.Add(new Ticket
        {
            CompanyId = seededData.CompanyId,
            CreatedByPersonId = seededData.RequesterId,
            AssignedTechnicianId = seededData.TechAId,
            Title = "Ticket a moi",
            Description = "Visible pour tech A",
            Category = TicketCategory.Software,
            Priority = TicketPriority.Normal,
            Status = TicketStatus.InProgress,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        dbContext.Tickets.Add(new Ticket
        {
            CompanyId = seededData.CompanyId,
            CreatedByPersonId = seededData.RequesterId,
            AssignedTechnicianId = seededData.TechBId,
            Title = "Ticket autre technicien",
            Description = "Ne doit pas apparaitre",
            Category = TicketCategory.Access,
            Priority = TicketPriority.High,
            Status = TicketStatus.InProgress,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var service = new TicketService(dbContext);

        var board = await service.GetBoardAsync(
            seededData.CompanyId,
            DemoRole.SupportTechnician,
            seededData.TechAId,
            searchTerm: null,
            statusFilter: null,
            priorityFilter: null,
            categoryFilter: null,
            assignedTechnicianIdFilter: null,
            createdFromDate: null,
            createdToDate: null,
            onlyMine: false);

        Assert.NotNull(board);
        Assert.Equal(2, board!.Tickets.Count);
        Assert.Contains(board.Tickets, ticket => ticket.Title == "Ticket demo");
        Assert.Contains(board.Tickets, ticket => ticket.Title == "Ticket a moi");
        Assert.DoesNotContain(board.Tickets, ticket => ticket.Title == "Ticket autre technicien");
    }

    [Fact]
    public async Task CreateAsync_AddsCreationHistoryEntry()
    {
        await using var dbContext = CreateDbContext();
        var seededData = await SeedScenarioAsync(dbContext, assignedTechnicianId: null, status: TicketStatus.New);
        var service = new TicketService(dbContext);

        var createdTicketId = await service.CreateAsync(
            seededData.CompanyId,
            seededData.RequesterId,
            new CreateTicketCommand("Nouveau ticket", "Description test", TicketCategory.Access, TicketPriority.High));

        var activities = await dbContext.TicketActivities
            .Where(x => x.TicketId == createdTicketId)
            .ToListAsync();

        Assert.Single(activities);
        Assert.Equal(TicketActivityType.Created, activities[0].ActivityType);
        Assert.Contains("Acces", activities[0].Description);
        Assert.Contains("Haute", activities[0].Description);
    }

    [Fact]
    public async Task GetBoardAsync_AppliesAdvancedFilters()
    {
        await using var dbContext = CreateDbContext();
        var seededData = await SeedScenarioAsync(dbContext, assignedTechnicianId: null, status: TicketStatus.New);

        dbContext.Tickets.Add(new Ticket
        {
            CompanyId = seededData.CompanyId,
            CreatedByPersonId = seededData.RequesterId,
            AssignedTechnicianId = seededData.TechAId,
            AssignedAtUtc = DateTime.UtcNow.AddHours(-1),
            Title = "Incident acces badge",
            Description = "Badge inactive",
            Category = TicketCategory.Access,
            Priority = TicketPriority.High,
            Status = TicketStatus.InProgress,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-1)
        });

        dbContext.Tickets.Add(new Ticket
        {
            CompanyId = seededData.CompanyId,
            CreatedByPersonId = seededData.RequesterId,
            AssignedTechnicianId = null,
            Title = "Sujet logiciel secondaire",
            Description = "Question simple",
            Category = TicketCategory.Software,
            Priority = TicketPriority.Low,
            Status = TicketStatus.New,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-5),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-5)
        });

        await dbContext.SaveChangesAsync();

        var service = new TicketService(dbContext);

        var board = await service.GetBoardAsync(
            seededData.CompanyId,
            DemoRole.CompanyAdmin,
            personId: null,
            searchTerm: "badge",
            statusFilter: TicketStatus.InProgress,
            priorityFilter: TicketPriority.High,
            categoryFilter: TicketCategory.Access,
            assignedTechnicianIdFilter: seededData.TechAId,
            createdFromDate: DateTime.UtcNow.AddDays(-2),
            createdToDate: DateTime.UtcNow,
            onlyMine: false);

        Assert.NotNull(board);
        Assert.Single(board!.Tickets);
        Assert.Equal("Incident acces badge", board.Tickets[0].Title);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<SeededScenario> SeedScenarioAsync(
        ApplicationDbContext dbContext,
        int? assignedTechnicianId,
        TicketStatus status)
    {
        var company = new Company
        {
            Name = "Contoso Test",
            Slug = Guid.NewGuid().ToString("N")
        };

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var requester = new DemoPerson
        {
            CompanyId = company.Id,
            Role = DemoRole.EndUser,
            FullName = "Lucas Bernard",
            JobTitle = "Collaborateur",
            Department = "Finance"
        };

        var techA = new DemoPerson
        {
            CompanyId = company.Id,
            Role = DemoRole.SupportTechnician,
            FullName = "Sarah Martin",
            JobTitle = "Technicienne support",
            Department = "Support"
        };

        var techB = new DemoPerson
        {
            CompanyId = company.Id,
            Role = DemoRole.SupportTechnician,
            FullName = "Karim Diallo",
            JobTitle = "Technicien support",
            Department = "Support"
        };

        dbContext.DemoPeople.AddRange(requester, techA, techB);
        await dbContext.SaveChangesAsync();

        var effectiveAssignedTechnicianId = assignedTechnicianId switch
        {
            2 => techA.Id,
            3 => techB.Id,
            _ => assignedTechnicianId
        };

        var ticket = new Ticket
        {
            CompanyId = company.Id,
            CreatedByPersonId = requester.Id,
            AssignedTechnicianId = effectiveAssignedTechnicianId,
            AssignedAtUtc = effectiveAssignedTechnicianId.HasValue ? DateTime.UtcNow.AddHours(-2) : null,
            Title = "Ticket demo",
            Description = "Besoin d'aide",
            Category = TicketCategory.Software,
            Priority = TicketPriority.Normal,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        ticket.Activities.Add(new TicketActivity
        {
            ActorPersonId = requester.Id,
            ActivityType = TicketActivityType.Created,
            Description = "Ticket cree avec la categorie Logiciel et la priorite Normale.",
            CreatedAtUtc = ticket.CreatedAtUtc
        });

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        return new SeededScenario(company.Id, requester.Id, techA.Id, techB.Id, ticket.Id);
    }

    private sealed record SeededScenario(
        int CompanyId,
        int RequesterId,
        int TechAId,
        int TechBId,
        int TicketId);
}
