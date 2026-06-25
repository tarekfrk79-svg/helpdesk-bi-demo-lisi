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
        var companyRows = await _dbContext.Companies
            .AsNoTracking()
            .OrderByDescending(company => company.CreatedAtUtc)
            .Select(company => new
            {
                company.Id,
                company.Name,
                company.Slug,
                AccessCode = company.AccessCodes
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Code)
                    .FirstOrDefault() ?? string.Empty,
                company.IsActive,
                UsageCount = company.AccessCodes.Sum(x => x.UsageCount),
                LastUsedAtUtc = company.AccessCodes.Max(x => x.LastUsedAtUtc),
                TicketCount = company.Tickets.Count,
                PersonCount = company.People.Count,
                company.CreatedAtUtc,
                company.LastResetAtUtc
            })
            .ToListAsync(cancellationToken);

        var companyIds = companyRows.Select(x => x.Id).ToList();

        var accountRows = await _dbContext.DemoPeople
            .AsNoTracking()
            .Where(person => companyIds.Contains(person.CompanyId) && person.IsActive)
            .OrderBy(person => person.CompanyId)
            .ThenBy(person => person.Role)
            .ThenBy(person => person.FullName)
            .Select(person => new
            {
                person.CompanyId,
                Account = new OwnerPersonAccessSummaryDto(
                    person.Id,
                    person.FullName,
                    DemoRoleFormatter.ToDisplayLabel(person.Role),
                    person.JobTitle,
                    person.Department,
                    person.LastSignedInAtUtc)
            })
            .ToListAsync(cancellationToken);

        var accountsByCompany = accountRows
            .GroupBy(x => x.CompanyId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<OwnerPersonAccessSummaryDto>)group
                    .Select(x => x.Account)
                    .ToList());

        var companies = companyRows
            .Select(company => new OwnerCompanySummaryDto(
                company.Id,
                company.Name,
                company.Slug,
                company.AccessCode,
                company.IsActive,
                company.UsageCount,
                company.LastUsedAtUtc,
                company.TicketCount,
                company.PersonCount,
                company.CreatedAtUtc,
                company.LastResetAtUtc,
                accountsByCompany.TryGetValue(company.Id, out var accounts)
                    ? accounts
                    : []))
            .ToList();

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
