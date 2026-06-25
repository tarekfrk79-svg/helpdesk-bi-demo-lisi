using HelpDeskBiDemo.Domain.Entities;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using HelpDeskBiDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpDeskBiDemo.Tests;

public sealed class DemoCompanyServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_ForCompanyAdmin_ComputesAdminMetrics()
    {
        await using var dbContext = CreateDbContext();

        var company = new Company
        {
            Name = "Contoso Support Demo",
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
            Title = "Ticket urgent sans technicien",
            Description = "Prioritaire",
            Category = TicketCategory.Hardware,
            Priority = TicketPriority.Urgent,
            Status = TicketStatus.New,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-6),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-6)
        });

        dbContext.Tickets.Add(new Ticket
        {
            CompanyId = company.Id,
            CreatedByPersonId = requester.Id,
            AssignedTechnicianId = tech.Id,
            AssignedAtUtc = DateTime.UtcNow.AddHours(-5),
            Title = "Ticket resolu",
            Description = "Traite",
            Category = TicketCategory.Software,
            Priority = TicketPriority.Normal,
            Status = TicketStatus.Resolved,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-8),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-2),
            ResolvedAtUtc = DateTime.UtcNow.AddHours(-2)
        });

        await dbContext.SaveChangesAsync();

        var service = new DemoCompanyService(dbContext);
        var dashboard = await service.GetDashboardAsync(company.Id, DemoRole.CompanyAdmin, admin.Id);

        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard!.OverdueTickets);
        Assert.Equal(1, dashboard.UnassignedTickets);
        Assert.True(dashboard.ResolutionRatePercent > 0);
        Assert.Single(dashboard.TechnicianWorkloads);
        Assert.Single(dashboard.OverdueTicketAlerts);
        Assert.Single(dashboard.UnassignedTicketAlerts);
    }

    [Fact]
    public async Task MarkPersonLastAccessAsync_UpdatesDemoPersonTimestamp()
    {
        await using var dbContext = CreateDbContext();

        var company = new Company
        {
            Name = "Contoso Support Demo",
            Slug = Guid.NewGuid().ToString("N")
        };

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var technician = new DemoPerson
        {
            CompanyId = company.Id,
            Role = DemoRole.SupportTechnician,
            FullName = "Karim Benali",
            JobTitle = "Technicien support",
            Department = "Support"
        };

        dbContext.DemoPeople.Add(technician);
        await dbContext.SaveChangesAsync();

        var service = new DemoCompanyService(dbContext);
        await service.MarkPersonLastAccessAsync(company.Id, technician.Id, DemoRole.SupportTechnician);

        var refreshed = await dbContext.DemoPeople.SingleAsync(x => x.Id == technician.Id);
        Assert.True(refreshed.LastSignedInAtUtc.HasValue);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
