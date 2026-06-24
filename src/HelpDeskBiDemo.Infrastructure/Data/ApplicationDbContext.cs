using HelpDeskBiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CodeUsageLog> CodeUsageLogs => Set<CodeUsageLog>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<CompanyAccessCode> CompanyAccessCodes => Set<CompanyAccessCode>();

    public DbSet<DemoPerson> DemoPeople => Set<DemoPerson>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    public DbSet<TicketActivity> TicketActivities => Set<TicketActivity>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
