using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Application.Common.Access;
using HelpDeskBiDemo.Infrastructure.Data;
using HelpDeskBiDemo.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class AccessService : IAccessService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly OwnerOptions _ownerOptions;

    public AccessService(ApplicationDbContext dbContext, IOptions<OwnerOptions> ownerOptions)
    {
        _dbContext = dbContext;
        _ownerOptions = ownerOptions.Value;
    }

    public async Task<AccessCodeResolution> ResolveAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return new AccessCodeResolution(AccessTargetType.Invalid, ErrorMessage: "Veuillez saisir un code.");
        }

        if (!string.IsNullOrWhiteSpace(_ownerOptions.AccessCode) &&
            normalizedCode == _ownerOptions.AccessCode.Trim().ToUpperInvariant())
        {
            return new AccessCodeResolution(AccessTargetType.Owner);
        }

        var accessCode = await _dbContext.CompanyAccessCodes
            .Include(x => x.Company)
            .FirstOrDefaultAsync(
                x => x.Code == normalizedCode &&
                     x.IsActive &&
                     x.Company != null &&
                     x.Company.IsActive,
                cancellationToken);

        if (accessCode is null || accessCode.Company is null)
        {
            return new AccessCodeResolution(
                AccessTargetType.Invalid,
                ErrorMessage: "Code invalide ou entreprise desactivee.");
        }

        accessCode.UsageCount++;
        accessCode.LastUsedAtUtc = DateTime.UtcNow;
        accessCode.UpdatedAtUtc = DateTime.UtcNow;
        accessCode.UsageLogs.Add(new Domain.Entities.CodeUsageLog());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AccessCodeResolution(
            AccessTargetType.Company,
            accessCode.CompanyId,
            accessCode.Company.Name);
    }
}
