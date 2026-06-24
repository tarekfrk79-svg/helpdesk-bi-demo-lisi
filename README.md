# HelpDesk BI Demo

HelpDesk BI Demo is a public demonstration project built with C#, ASP.NET Core MVC, .NET 8, and SQL Server to showcase web development, user support workflows, project organization, and a first BI export flow for Tableau.

## Current Scope

The repository is being built as a recruiter-friendly MVP with:

- a public landing page with one access code field;
- an Owner space secured by an environment variable;
- company-specific demo codes for recruiters;
- three demo role families per company: Admin, Technicien, Utilisateur;
- a seeded demo company with sample tickets and comments;
- dashboards for the active company and role;
- ticket listing, detail view, comments, and first role-based actions;
- CSV export and Tableau-ready data extraction.

## Architecture

The solution keeps a clean structure without overengineering:

```text
HelpDeskBiDemo.sln
src/
  HelpDeskBiDemo.Web/
  HelpDeskBiDemo.Application/
  HelpDeskBiDemo.Domain/
  HelpDeskBiDemo.Infrastructure/
tests/
  HelpDeskBiDemo.Tests/
```

Project responsibilities:

- `HelpDeskBiDemo.Web`: ASP.NET Core MVC host, Razor views, controllers, and UI assets.
- `HelpDeskBiDemo.Application`: application services, use cases, and orchestration logic.
- `HelpDeskBiDemo.Domain`: core business entities and domain rules.
- `HelpDeskBiDemo.Infrastructure`: SQL Server, Entity Framework Core, seeding, and infrastructure services.
- `HelpDeskBiDemo.Tests`: test project for domain and application scenarios.

## Deployment Target

The target deployment path is:

- `Azure App Service` for the web application;
- `Azure SQL Database` for the SQL Server-compatible database;
- configuration through Azure application settings and environment variables.

Expected environment variables:

- `ConnectionStrings__DefaultConnection`
- `Owner__AccessCode`
- `Demo__SeedData`
- `ASPNETCORE_ENVIRONMENT`

## Local Run

Recommended local prerequisites:

- .NET 8 SDK
- SQL Server LocalDB, SQL Server Express, or Azure SQL for remote testing

Typical commands:

```bash
bash scripts/build-solution.sh
bash scripts/run-ef-update.sh
bash scripts/publish-web.sh
```

Suggested local environment variables:

```bash
ConnectionStrings__DefaultConnection="Server=localhost;Database=HelpDeskBiDemo;Trusted_Connection=True;TrustServerCertificate=True"
Owner__AccessCode="OWNER-DEMO-2026"
Demo__SeedData="true"
ASPNETCORE_ENVIRONMENT="Development"
```

## Azure Deployment Outline

Target stack:

- Azure App Service for the MVC application
- Azure SQL Database for the data layer

High-level steps:

1. Create an Azure SQL Database and server.
2. Allow Azure services to reach the SQL server.
3. Create an App Service plan and a Web App targeting .NET 8.
4. Set the application settings:
   - `ConnectionStrings__DefaultConnection`
   - `Owner__AccessCode`
   - `Demo__SeedData`
   - `ASPNETCORE_ENVIRONMENT`
5. Deploy the application from a publish ZIP package.
6. Run EF Core migrations against the Azure SQL connection string, or let the application apply them at startup.

Deployment helpers included in this repository:

- `bash scripts/create-azure-zip.sh`
- `bash scripts/run-ef-update.sh`
- `deploy/azure-deploy.md`
- `deploy/azure-app-settings.example.txt`
- `deploy/sql/001_initial_idempotent.sql`

Recommended demo setup:

- keep `Owner__AccessCode` private;
- keep recruiter companies separate;
- enable `Demo__SeedData=true` only for first startup or controlled resets.

## Tableau Export

The Admin company view and the Owner view can export tickets as CSV.

The export currently includes:

- company
- ticket id
- title
- description
- category
- priority
- status
- requester
- assigned technician
- created date
- updated date
- resolved date
- resolution time in hours
- comment count

Suggested Tableau workflow:

1. Open Tableau and connect to a text/CSV file.
2. Select the exported `HelpDesk BI Demo` CSV.
3. Convert the `CreatedAtUtc`, `UpdatedAtUtc`, and `ResolvedAtUtc` columns to date-time fields.
4. Build simple visuals:
   - tickets by status
   - tickets by category
   - tickets by priority
   - average resolution hours
   - tickets by technician
5. Create a dashboard that combines operational support KPIs and BI storytelling.

## Skills Demonstrated

This project is designed to demonstrate:

- C# and ASP.NET Core MVC development
- .NET 8 project structure and dependency injection
- SQL Server and Entity Framework Core modeling
- user support workflows and ticket lifecycle management
- BI export preparation for Tableau Software
- project organization and cloud deployment readiness

## Status

The repository already includes:

- the solution skeleton and MVC host;
- the core domain model;
- the Entity Framework Core SQL Server context and mappings;
- the first EF Core migration and idempotent SQL script;
- automatic startup initialization when a SQL Server connection string is configured;
- the Owner flow and company generation logic;
- the company role selection flow;
- the first working ticket workflow for end users, admins, and technicians;
- local helper scripts for build, publish, migrations, and Azure packaging;
- a live Azure deployment path that has already been validated on App Service + Azure SQL.

Still planned in the next increments:

- richer dashboard statistics;
- dedicated business tests and UI refinements;
- optional GitHub Actions automation once the repository is published.

Environment note:

- in this workspace, the most reliable build commands use `-m:1` and `BuildInParallel=false`;
- the helper scripts already apply these flags.
