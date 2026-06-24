using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Owner;
using HelpDeskBiDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class OwnerManagementService : IOwnerManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DemoCompanyFactory _companyFactory;

    public OwnerManagementService(ApplicationDbContext dbContext, DemoCompanyFactory companyFactory)
    {
        _dbContext = dbContext;
        _companyFactory = companyFactory;
    }

    public async Task<OwnerDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _dbContext.Companies
            .AsNoTracking()
            .OrderByDescending(company => company.CreatedAtUtc)
            .Select(company => new OwnerCompanySummaryDto(
                company.Id,
                company.Name,
                company.Slug,
                company.AccessCodes
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Code)
                    .FirstOrDefault() ?? string.Empty,
                company.IsActive,
                company.AccessCodes.Sum(x => x.UsageCount),
                company.AccessCodes.Max(x => x.LastUsedAtUtc),
                company.Tickets.Count,
                company.People.Count,
                company.CreatedAtUtc,
                company.LastResetAtUtc))
            .ToListAsync(cancellationToken);

        return new OwnerDashboardDto(
            companies.Count,
            companies.Count(x => x.IsActive),
            companies.Sum(x => x.TicketCount),
            companies.Sum(x => x.UsageCount),
            companies);
    }

    public async Task CreateCompanyAsync(string companyName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new InvalidOperationException("Le nom de l'entreprise est requis.");
        }

        var company = await _companyFactory.BuildAsync(
            companyName.Trim(),
            requestedAccessCode: null,
            cancellationToken);

        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetCompanyActivationAsync(
        int companyId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .Include(x => x.AccessCodes)
            .FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken);

        if (company is null)
        {
            return;
        }

        company.IsActive = isActive;
        company.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var accessCode in company.AccessCodes)
        {
            accessCode.IsActive = isActive;
            accessCode.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetCompanyDataAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .Include(x => x.Tickets)
                .ThenInclude(x => x.Activities)
            .Include(x => x.Tickets)
                .ThenInclude(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken);

        if (company is null)
        {
            return;
        }

        var activities = company.Tickets.SelectMany(x => x.Activities).ToList();
        var comments = company.Tickets.SelectMany(x => x.Comments).ToList();

        _dbContext.TicketActivities.RemoveRange(activities);
        _dbContext.TicketComments.RemoveRange(comments);
        _dbContext.Tickets.RemoveRange(company.Tickets);

        company.LastResetAtUtc = DateTime.UtcNow;
        company.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken);

        if (company is null)
        {
            return;
        }

        _dbContext.Companies.Remove(company);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
