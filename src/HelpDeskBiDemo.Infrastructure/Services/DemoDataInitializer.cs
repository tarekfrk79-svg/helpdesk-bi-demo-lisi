using HelpDeskBiDemo.Application.Abstractions;
using HelpDeskBiDemo.Domain.Entities;
using HelpDeskBiDemo.Domain.Enums;
using HelpDeskBiDemo.Infrastructure.Data;
using HelpDeskBiDemo.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDeskBiDemo.Infrastructure.Services;

internal sealed class DemoDataInitializer : IDemoDataInitializer
{
    private const string DefaultDemoCompanyCode = "CONTOSO-DEMO";

    private readonly ApplicationDbContext _dbContext;
    private readonly DemoCompanyFactory _companyFactory;
    private readonly DemoOptions _demoOptions;
    private readonly ILogger<DemoDataInitializer> _logger;

    public DemoDataInitializer(
        ApplicationDbContext dbContext,
        DemoCompanyFactory companyFactory,
        IOptions<DemoOptions> demoOptions,
        ILogger<DemoDataInitializer> logger)
    {
        _dbContext = dbContext;
        _companyFactory = companyFactory;
        _demoOptions = demoOptions.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _dbContext.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Database initialization skipped because no connection string is configured.");
            return;
        }

        await _dbContext.Database.MigrateAsync(cancellationToken);

        if (!_demoOptions.SeedData || await _dbContext.Companies.AnyAsync(cancellationToken))
        {
            return;
        }

        var company = await _companyFactory.BuildAsync(
            "Contoso Support Demo",
            DefaultDemoCompanyCode,
            cancellationToken);

        SeedTickets(company);

        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded default demo company '{CompanyName}' with access code '{AccessCode}'.",
            company.Name,
            DefaultDemoCompanyCode);
    }

    private static void SeedTickets(Company company)
    {
        var techSarah = company.People.Single(x => x.FullName == "Sarah Martin");
        var techKarim = company.People.Single(x => x.FullName == "Karim Benali");
        var userAmelie = company.People.Single(x => x.FullName == "Amelie Durand");
        var userThomas = company.People.Single(x => x.FullName == "Thomas Leroy");
        var userNadia = company.People.Single(x => x.FullName == "Nadia Morel");

        var now = DateTime.UtcNow;
        var ticket1CreatedAt = now.AddHours(-8);
        var ticket1AssignedAt = now.AddHours(-2);
        var ticket2CreatedAt = now.AddHours(-6);
        var ticket3CreatedAt = now.AddDays(-3);
        var ticket3AssignedAt = now.AddDays(-2.5);
        var ticket3ResolvedAt = now.AddDays(-1);
        var ticket4CreatedAt = now.AddDays(-10);
        var ticket4AssignedAt = now.AddDays(-9);
        var ticket4ResolvedAt = now.AddDays(-7);

        var ticket1 = new Ticket
        {
            Title = "Impossible d'acceder au logiciel RH",
            Description = "Le compte semble bloque apres plusieurs tentatives de connexion.",
            Category = TicketCategory.Access,
            Priority = TicketPriority.High,
            Status = TicketStatus.InProgress,
            CreatedByPerson = userAmelie,
            AssignedTechnician = techSarah,
            CreatedAtUtc = ticket1CreatedAt,
            UpdatedAtUtc = ticket1AssignedAt,
            AssignedAtUtc = ticket1AssignedAt
        };

        ticket1.Activities.Add(new TicketActivity
        {
            ActorPerson = userAmelie,
            ActivityType = TicketActivityType.Created,
            Description = "Ticket cree avec la categorie Acces et la priorite Haute.",
            CreatedAtUtc = ticket1CreatedAt
        });
        ticket1.Activities.Add(new TicketActivity
        {
            ActorPerson = techSarah,
            ActivityType = TicketActivityType.AssignmentChanged,
            Description = $"Ticket assigne a {techSarah.FullName}.",
            CreatedAtUtc = ticket1AssignedAt
        });
        ticket1.Activities.Add(new TicketActivity
        {
            ActorPerson = techSarah,
            ActivityType = TicketActivityType.StatusChanged,
            Description = $"Statut passe de Nouveau a En cours. Action effectuee par {techSarah.FullName}.",
            CreatedAtUtc = ticket1AssignedAt
        });

        ticket1.Comments.Add(new TicketComment
        {
            AuthorPerson = userAmelie,
            Content = "Le probleme bloque la preparation des contrats.",
            CreatedAtUtc = ticket1CreatedAt
        });
        ticket1.Activities.Add(new TicketActivity
        {
            ActorPerson = userAmelie,
            ActivityType = TicketActivityType.CommentAdded,
            Description = "Commentaire ajoute sur le ticket.",
            CreatedAtUtc = ticket1CreatedAt
        });
        ticket1.Comments.Add(new TicketComment
        {
            AuthorPerson = techSarah,
            Content = "Le ticket est pris en charge, je verifie les droits Active Directory.",
            CreatedAtUtc = ticket1AssignedAt
        });
        ticket1.Activities.Add(new TicketActivity
        {
            ActorPerson = techSarah,
            ActivityType = TicketActivityType.CommentAdded,
            Description = "Commentaire ajoute sur le ticket.",
            CreatedAtUtc = ticket1AssignedAt
        });

        var ticket2 = new Ticket
        {
            Title = "Ecran noir sur un poste commercial",
            Description = "Le poste demarre mais l'ecran reste noir apres la connexion.",
            Category = TicketCategory.Hardware,
            Priority = TicketPriority.Urgent,
            Status = TicketStatus.New,
            CreatedByPerson = userThomas,
            CreatedAtUtc = ticket2CreatedAt,
            UpdatedAtUtc = ticket2CreatedAt
        };

        ticket2.Activities.Add(new TicketActivity
        {
            ActorPerson = userThomas,
            ActivityType = TicketActivityType.Created,
            Description = "Ticket cree avec la categorie Materiel et la priorite Urgente.",
            CreatedAtUtc = ticket2CreatedAt
        });

        var ticket3 = new Ticket
        {
            Title = "Erreur export comptable mensuel",
            Description = "L'export plante avec un message d'erreur non documente.",
            Category = TicketCategory.Bug,
            Priority = TicketPriority.Normal,
            Status = TicketStatus.Resolved,
            CreatedByPerson = userNadia,
            AssignedTechnician = techKarim,
            CreatedAtUtc = ticket3CreatedAt,
            UpdatedAtUtc = ticket3ResolvedAt,
            AssignedAtUtc = ticket3AssignedAt,
            ResolvedAtUtc = ticket3ResolvedAt
        };

        ticket3.Activities.Add(new TicketActivity
        {
            ActorPerson = userNadia,
            ActivityType = TicketActivityType.Created,
            Description = "Ticket cree avec la categorie Bug et la priorite Normale.",
            CreatedAtUtc = ticket3CreatedAt
        });
        ticket3.Activities.Add(new TicketActivity
        {
            ActorPerson = techKarim,
            ActivityType = TicketActivityType.AssignmentChanged,
            Description = $"Ticket assigne a {techKarim.FullName}.",
            CreatedAtUtc = ticket3AssignedAt
        });
        ticket3.Activities.Add(new TicketActivity
        {
            ActorPerson = techKarim,
            ActivityType = TicketActivityType.StatusChanged,
            Description = $"Statut passe de Nouveau a Resolu. Action effectuee par {techKarim.FullName}.",
            CreatedAtUtc = ticket3ResolvedAt
        });

        ticket3.Comments.Add(new TicketComment
        {
            AuthorPerson = techKarim,
            Content = "Correctif applique et export relance avec succes.",
            CreatedAtUtc = ticket3ResolvedAt
        });
        ticket3.Activities.Add(new TicketActivity
        {
            ActorPerson = techKarim,
            ActivityType = TicketActivityType.CommentAdded,
            Description = "Commentaire ajoute sur le ticket.",
            CreatedAtUtc = ticket3ResolvedAt
        });

        var ticket4 = new Ticket
        {
            Title = "Installation d'un nouvel outil de reporting",
            Description = "Besoin d'acces a l'outil sur le poste de direction.",
            Category = TicketCategory.Software,
            Priority = TicketPriority.Low,
            Status = TicketStatus.Closed,
            CreatedByPerson = userAmelie,
            AssignedTechnician = techSarah,
            CreatedAtUtc = ticket4CreatedAt,
            UpdatedAtUtc = ticket4ResolvedAt,
            AssignedAtUtc = ticket4AssignedAt,
            ResolvedAtUtc = ticket4ResolvedAt
        };

        ticket4.Activities.Add(new TicketActivity
        {
            ActorPerson = userAmelie,
            ActivityType = TicketActivityType.Created,
            Description = "Ticket cree avec la categorie Logiciel et la priorite Basse.",
            CreatedAtUtc = ticket4CreatedAt
        });
        ticket4.Activities.Add(new TicketActivity
        {
            ActorPerson = techSarah,
            ActivityType = TicketActivityType.AssignmentChanged,
            Description = $"Ticket assigne a {techSarah.FullName}.",
            CreatedAtUtc = ticket4AssignedAt
        });
        ticket4.Activities.Add(new TicketActivity
        {
            ActorPerson = techSarah,
            ActivityType = TicketActivityType.StatusChanged,
            Description = $"Statut passe de Nouveau a Clos. Action effectuee par {techSarah.FullName}.",
            CreatedAtUtc = ticket4ResolvedAt
        });

        company.Tickets.Add(ticket1);
        company.Tickets.Add(ticket2);
        company.Tickets.Add(ticket3);
        company.Tickets.Add(ticket4);
    }
}
