# Azure Deployment Guide

Official Azure account creation link:

- `https://azure.microsoft.com/fr-fr/free/`

## What Is Already Ready

- the web app builds successfully with the local .NET 8 SDK bundled in this repository;
- the web app publishes successfully in `Release`;
- the first EF Core migration is committed in `src/HelpDeskBiDemo.Infrastructure/Data/Migrations`;
- an idempotent SQL script is available at `deploy/sql/001_initial_idempotent.sql`;
- reusable scripts are available in `scripts/`.

## Azure Portal Preparation

Create these resources in Azure Portal:

1. A resource group for the demo.
2. An Azure SQL logical server.
3. An Azure SQL Database on that server.
4. A firewall rule that allows your public IP to connect to the SQL server.
5. An App Service plan.
6. A Web App configured for `.NET 8`.

Keep the SQL admin login and password safe. I do not need them. You will only paste the final connection string into Azure or into your own terminal when running migrations.

## Required App Settings

In the Web App, add application settings with the exact names below:

- `ConnectionStrings__DefaultConnection`
- `Owner__AccessCode`
- `Demo__SeedData`
- `ASPNETCORE_ENVIRONMENT`

You can copy `deploy/azure-app-settings.example.txt` and replace the placeholders.

Recommended initial values:

- `Demo__SeedData=true`
- `ASPNETCORE_ENVIRONMENT=Production`

## Build And Package

Create the deployable ZIP package locally:

```bash
bash scripts/create-azure-zip.sh
```

This produces:

- `artifacts/publish/web`
- `artifacts/HelpDeskBiDemo.Web.zip`

## Deploy To App Service

With Azure CLI, deploy the ZIP package:

```bash
az webapp deploy --resource-group <resource-group> --name <web-app-name> --src-path artifacts/HelpDeskBiDemo.Web.zip
```

## Apply Database Migrations

From your terminal, point EF Core to your Azure SQL connection string and run:

```bash
export ConnectionStrings__DefaultConnection="Server=tcp:<sql-server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Persist Security Info=False;User ID=<sql-admin-user>;Password=<sql-admin-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
bash scripts/run-ef-update.sh
```

If you prefer a SQL script instead of EF commands, use:

- `deploy/sql/001_initial_idempotent.sql`

## First Online Check

After deployment:

1. Open the Web App URL.
2. Test the owner access code.
3. Test one recruiter company code.
4. Confirm that seeded tickets appear.
5. If everything is correct, set `Demo__SeedData=false` to avoid accidental reseeding on later startups.
