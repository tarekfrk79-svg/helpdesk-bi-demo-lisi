using HelpDeskBiDemo.Domain.Entities;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using HelpDeskBiDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpDeskBiDemo.Tests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task GetCenterAsync_ReturnsAdminNotificationsAndLiveAlerts()
    {
        await using var dbContext = CreateDbContext();
        var seeded = await SeedScenarioAsync(dbContext);

        dbContext.Notifications.Add(new Notification
        {
            CompanyId = seeded.CompanyId,
            TicketId = seeded.TicketId,
            NotificationType = NotificationType.TicketCreated,
            RecipientRole = DemoRole.CompanyAdmin,
            Title = "Nouveau ticket utilisateur",
            Message = "Lucas Bernard a cree un ticket.",
            ActionUrl = $"/Tickets/Details/{seeded.TicketId}",
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-30)
        });

        await dbContext.SaveChangesAsync();

        var service = new NotificationService(dbContext);
        var center = await service.GetCenterAsync(
            seeded.CompanyId,
            DemoRole.CompanyAdmin,
            seeded.AdminId,
            "Admin Demo");

        Assert.NotNull(center);
        Assert.Single(center!.Notifications);
        Assert.NotEmpty(center.LiveAlerts);
        Assert.Equal(1, center.UnreadCount);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_OnlyMarksScopedNotifications()
    {
        await using var dbContext = CreateDbContext();
        var seeded = await SeedScenarioAsync(dbContext);

        dbContext.Notifications.AddRange(
            new Notification
            {
                CompanyId = seeded.CompanyId,
                TicketId = seeded.TicketId,
                NotificationType = NotificationType.TicketCommentAdded,
                RecipientRole = DemoRole.EndUser,
                RecipientPersonId = seeded.RequesterId,
                Title = "Commentaire",
                Message = "Mise a jour ticket",
                ActionUrl = $"/Tickets/Details/{seeded.TicketId}",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
            },
            new Notification
            {
                CompanyId = seeded.CompanyId,
                TicketId = seeded.TicketId,
                NotificationType = NotificationType.TicketAssigned,
                RecipientRole = DemoRole.SupportTechnician,
                RecipientPersonId = seeded.TechId,
                Title = "Assignation",
                Message = "Ticket pour le technicien",
                ActionUrl = $"/Tickets/Details/{seeded.TicketId}",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-9)
            });

        await dbContext.SaveChangesAsync();

        var service = new NotificationService(dbContext);
        await service.MarkAllAsReadAsync(seeded.CompanyId, DemoRole.EndUser, seeded.RequesterId);

        var userNotification = await dbContext.Notifications.SingleAsync(x => x.RecipientRole == DemoRole.EndUser);
        var techNotification = await dbContext.Notifications.SingleAsync(x => x.RecipientRole == DemoRole.SupportTechnician);

        Assert.True(userNotification.IsRead);
        Assert.False(techNotification.IsRead);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<SeededNotificationScenario> SeedScenarioAsync(ApplicationDbContext dbContext)
    {
        var company = new Company
        {
            Name = "Contoso Notifications",
            Slug = Guid.NewGuid().ToString("N")
        };

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var admin = new DemoPerson
        {
            CompanyId = company.Id,
            Role = DemoRole.CompanyAdmin,
            FullName = "Admin Demo",
            JobTitle = "Responsable support",
            Department = "Support"
        };

        var tech = new DemoPerson
        {
            CompanyId = company.Id,
            Role = DemoRole.SupportTechnician,
            FullName = "Sarah Martin",
            JobTitle = "Technicienne support",
            Department = "Support"
        };

        var requester = new DemoPerson
        {
            CompanyId = company.Id,
            Role = DemoRole.EndUser,
            FullName = "Lucas Bernard",
            JobTitle = "Collaborateur",
            Department = "Finance"
        };

        dbContext.DemoPeople.AddRange(admin, tech, requester);
        await dbContext.SaveChangesAsync();

        dbContext.Tickets.Add(new Ticket
        {
            CompanyId = company.Id,
            CreatedByPersonId = requester.Id,
            Title = "Ticket urgent",
            Description = "Urgent et non assigne",
            Category = TicketCategory.Hardware,
            Priority = TicketPriority.Urgent,
            Status = TicketStatus.New,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-6),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-6)
        });

        await dbContext.SaveChangesAsync();

        var ticketId = await dbContext.Tickets.Select(x => x.Id).SingleAsync();
        return new SeededNotificationScenario(company.Id, admin.Id, tech.Id, requester.Id, ticketId);
    }

    private sealed record SeededNotificationScenario(
        int CompanyId,
        int AdminId,
        int TechId,
        int RequesterId,
        int TicketId);
}
