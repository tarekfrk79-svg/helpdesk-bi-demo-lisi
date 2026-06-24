IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(120) NOT NULL,
        [Slug] nvarchar(120) NOT NULL,
        [IsActive] bit NOT NULL,
        [LastResetAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE TABLE [CompanyAccessCodes] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [Code] nvarchar(32) NOT NULL,
        [IsActive] bit NOT NULL,
        [UsageCount] int NOT NULL,
        [LastUsedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_CompanyAccessCodes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanyAccessCodes_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE TABLE [DemoPeople] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [Role] int NOT NULL,
        [FullName] nvarchar(120) NOT NULL,
        [JobTitle] nvarchar(120) NOT NULL,
        [Department] nvarchar(80) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_DemoPeople] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DemoPeople_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE TABLE [CodeUsageLogs] (
        [Id] int NOT NULL IDENTITY,
        [CompanyAccessCodeId] int NOT NULL,
        [UsedAtUtc] datetime2 NOT NULL,
        [Source] nvarchar(64) NOT NULL,
        CONSTRAINT [PK_CodeUsageLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CodeUsageLogs_CompanyAccessCodes_CompanyAccessCodeId] FOREIGN KEY ([CompanyAccessCodeId]) REFERENCES [CompanyAccessCodes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE TABLE [Tickets] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [CreatedByPersonId] int NOT NULL,
        [AssignedTechnicianId] int NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(4000) NOT NULL,
        [Category] int NOT NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [ResolvedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tickets_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Tickets_DemoPeople_AssignedTechnicianId] FOREIGN KEY ([AssignedTechnicianId]) REFERENCES [DemoPeople] ([Id]),
        CONSTRAINT [FK_Tickets_DemoPeople_CreatedByPersonId] FOREIGN KEY ([CreatedByPersonId]) REFERENCES [DemoPeople] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketComments] (
        [Id] int NOT NULL IDENTITY,
        [TicketId] int NOT NULL,
        [AuthorPersonId] int NOT NULL,
        [Content] nvarchar(4000) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketComments_DemoPeople_AuthorPersonId] FOREIGN KEY ([AuthorPersonId]) REFERENCES [DemoPeople] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketComments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CodeUsageLogs_CompanyAccessCodeId] ON [CodeUsageLogs] ([CompanyAccessCodeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Companies_Slug] ON [Companies] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CompanyAccessCodes_Code] ON [CompanyAccessCodes] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CompanyAccessCodes_CompanyId] ON [CompanyAccessCodes] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DemoPeople_CompanyId] ON [DemoPeople] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_AuthorPersonId] ON [TicketComments] ([AuthorPersonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_TicketId] ON [TicketComments] ([TicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_AssignedTechnicianId] ON [Tickets] ([AssignedTechnicianId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CompanyId_Category] ON [Tickets] ([CompanyId], [Category]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CompanyId_Status] ON [Tickets] ([CompanyId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CreatedByPersonId] ON [Tickets] ([CreatedByPersonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624205640_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260624205640_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624222912_AddTicketHistoryAndSlaTracking'
)
BEGIN
    ALTER TABLE [Tickets] ADD [AssignedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624222912_AddTicketHistoryAndSlaTracking'
)
BEGIN
    CREATE TABLE [TicketActivities] (
        [Id] int NOT NULL IDENTITY,
        [TicketId] int NOT NULL,
        [ActivityType] int NOT NULL,
        [ActorPersonId] int NULL,
        [Description] nvarchar(600) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketActivities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketActivities_DemoPeople_ActorPersonId] FOREIGN KEY ([ActorPersonId]) REFERENCES [DemoPeople] ([Id]),
        CONSTRAINT [FK_TicketActivities_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624222912_AddTicketHistoryAndSlaTracking'
)
BEGIN
    CREATE INDEX [IX_TicketActivities_ActorPersonId] ON [TicketActivities] ([ActorPersonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624222912_AddTicketHistoryAndSlaTracking'
)
BEGIN
    CREATE INDEX [IX_TicketActivities_TicketId_CreatedAtUtc] ON [TicketActivities] ([TicketId], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624222912_AddTicketHistoryAndSlaTracking'
)
BEGIN
    UPDATE Tickets
    SET AssignedAtUtc = CreatedAtUtc
    WHERE AssignedTechnicianId IS NOT NULL
      AND AssignedAtUtc IS NULL;
    INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
    SELECT
        t.Id,
        1,
        t.CreatedByPersonId,
        CONCAT(
            N'Ticket cree avec la categorie ',
            CASE t.Category
                WHEN 1 THEN N'Logiciel'
                WHEN 2 THEN N'Materiel'
                WHEN 3 THEN N'Acces'
                WHEN 4 THEN N'Bug'
                ELSE N'Autre'
            END,
            N' et la priorite ',
            CASE t.Priority
                WHEN 1 THEN N'Basse'
                WHEN 2 THEN N'Normale'
                WHEN 3 THEN N'Haute'
                WHEN 4 THEN N'Urgente'
                ELSE N'Normale'
            END,
            N'.'
        ),
        t.CreatedAtUtc
    FROM Tickets t;
    INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
    SELECT
        t.Id,
        2,
        t.AssignedTechnicianId,
        CONCAT(N'Ticket assigne a ', technician.FullName, N'.'),
        COALESCE(t.AssignedAtUtc, t.CreatedAtUtc)
    FROM Tickets t
    INNER JOIN DemoPeople technician ON technician.Id = t.AssignedTechnicianId
    WHERE t.AssignedTechnicianId IS NOT NULL;
    INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
    SELECT
        t.Id,
        3,
        COALESCE(t.AssignedTechnicianId, t.CreatedByPersonId),
        CONCAT(
            N'Statut passe de Nouveau a ',
            CASE t.Status
                WHEN 2 THEN N'En cours'
                WHEN 3 THEN N'Resolu'
                WHEN 4 THEN N'Clos'
                ELSE N'Nouveau'
            END,
            N'. Action effectuee par ',
            COALESCE(technician.FullName, requester.FullName),
            N'.'
        ),
        COALESCE(t.ResolvedAtUtc, t.UpdatedAtUtc, t.CreatedAtUtc)
    FROM Tickets t
    INNER JOIN DemoPeople requester ON requester.Id = t.CreatedByPersonId
    LEFT JOIN DemoPeople technician ON technician.Id = t.AssignedTechnicianId
    WHERE t.Status <> 1;
    INSERT INTO TicketActivities (TicketId, ActivityType, ActorPersonId, Description, CreatedAtUtc)
    SELECT
        comment.TicketId,
        4,
        comment.AuthorPersonId,
        N'Commentaire ajoute sur le ticket.',
        comment.CreatedAtUtc
    FROM TicketComments comment;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624222912_AddTicketHistoryAndSlaTracking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260624222912_AddTicketHistoryAndSlaTracking', N'8.0.0');
END;
GO

COMMIT;
GO

