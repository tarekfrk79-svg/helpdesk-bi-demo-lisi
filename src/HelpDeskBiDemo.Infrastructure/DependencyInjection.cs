using HelpDeskBiDemo.Infrastructure.Options;
using HelpDeskBiDemo.Infrastructure.Data;
using HelpDeskBiDemo.Infrastructure.Services;
using HelpDeskBiDemo.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.Configure<OwnerOptions>(configuration.GetSection(OwnerOptions.SectionName));
        services.Configure<DemoOptions>(configuration.GetSection(DemoOptions.SectionName));

        services.AddScoped<DemoCompanyFactory>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<ICsvExportService, CsvExportService>();
        services.AddScoped<IDemoCompanyService, DemoCompanyService>();
        services.AddScoped<IDemoDataInitializer, DemoDataInitializer>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOwnerManagementService, OwnerManagementService>();
        services.AddScoped<ITicketService, TicketService>();

        return services;
    }
}
