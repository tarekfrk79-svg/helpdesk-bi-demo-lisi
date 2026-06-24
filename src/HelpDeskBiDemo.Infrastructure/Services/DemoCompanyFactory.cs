using System.Text;
using HelpDeskBiDemo.Domain.Entities;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class DemoCompanyFactory
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Random _random = new();

    public DemoCompanyFactory(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Company> BuildAsync(
        string companyName,
        string? requestedAccessCode = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = companyName.Trim();
        var slug = await GenerateUniqueSlugAsync(normalizedName, cancellationToken);
        var accessCode = await ResolveAccessCodeAsync(normalizedName, requestedAccessCode, cancellationToken);

        var company = new Company
        {
            Name = normalizedName,
            Slug = slug
        };

        company.AccessCodes.Add(new CompanyAccessCode
        {
            Code = accessCode,
            IsActive = true
        });

        foreach (var person in CreateDefaultPeople())
        {
            company.People.Add(person);
        }

        return company;
    }

    private async Task<string> ResolveAccessCodeAsync(
        string companyName,
        string? requestedAccessCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestedAccessCode))
        {
            return await GenerateUniqueAccessCodeAsync(companyName, cancellationToken);
        }

        var normalizedCode = requestedAccessCode.Trim().ToUpperInvariant();
        var exists = await _dbContext.CompanyAccessCodes.AnyAsync(x => x.Code == normalizedCode, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"The access code '{normalizedCode}' is already in use.");
        }

        return normalizedCode;
    }

    private async Task<string> GenerateUniqueSlugAsync(string companyName, CancellationToken cancellationToken)
    {
        var slugBase = Slugify(companyName);
        var slug = slugBase;
        var index = 2;

        while (await _dbContext.Companies.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{slugBase}-{index}";
            index++;
        }

        return slug;
    }

    private async Task<string> GenerateUniqueAccessCodeAsync(string companyName, CancellationToken cancellationToken)
    {
        var prefix = BuildCodePrefix(companyName);
        string candidate;

        do
        {
            candidate = $"{prefix}-{GenerateToken(4)}";
        }
        while (await _dbContext.CompanyAccessCodes.AnyAsync(x => x.Code == candidate, cancellationToken));

        return candidate;
    }

    private static string Slugify(string value)
    {
        var buffer = new StringBuilder();
        var previousDash = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer.Append(character);
                previousDash = false;
                continue;
            }

            if (previousDash)
            {
                continue;
            }

            buffer.Append('-');
            previousDash = true;
        }

        var slug = buffer.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "company" : slug;
    }

    private static string BuildCodePrefix(string companyName)
    {
        var letters = new string(companyName
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());

        if (letters.Length >= 5)
        {
            return letters[..5];
        }

        return letters.PadRight(5, 'X');
    }

    private string GenerateToken(int length)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var chars = new char[length];

        for (var index = 0; index < length; index++)
        {
            chars[index] = alphabet[_random.Next(alphabet.Length)];
        }

        return new string(chars);
    }

    private static IReadOnlyList<DemoPerson> CreateDefaultPeople() =>
        new List<DemoPerson>
        {
            new()
            {
                Role = DemoRole.CompanyAdmin,
                FullName = "Julien Moreau",
                JobTitle = "Responsable informatique",
                Department = "IT"
            },
            new()
            {
                Role = DemoRole.SupportTechnician,
                FullName = "Sarah Martin",
                JobTitle = "Technicienne support",
                Department = "Support"
            },
            new()
            {
                Role = DemoRole.SupportTechnician,
                FullName = "Karim Benali",
                JobTitle = "Technicien support",
                Department = "Support"
            },
            new()
            {
                Role = DemoRole.EndUser,
                FullName = "Amelie Durand",
                JobTitle = "Collaboratrice RH",
                Department = "Ressources humaines"
            },
            new()
            {
                Role = DemoRole.EndUser,
                FullName = "Thomas Leroy",
                JobTitle = "Commercial",
                Department = "Commercial"
            },
            new()
            {
                Role = DemoRole.EndUser,
                FullName = "Nadia Morel",
                JobTitle = "Comptable",
                Department = "Comptabilite"
            },
            new()
            {
                Role = DemoRole.EndUser,
                FullName = "Lucas Bernard",
                JobTitle = "Coordinateur logistique",
                Department = "Logistique"
            },
            new()
            {
                Role = DemoRole.EndUser,
                FullName = "Emma Petit",
                JobTitle = "Assistante de direction",
                Department = "Direction"
            }
        };
}
