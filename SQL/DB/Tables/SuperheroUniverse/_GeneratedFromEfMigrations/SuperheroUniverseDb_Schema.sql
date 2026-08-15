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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtMissions] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Location] nvarchar(150) NULL,
        [Difficulty] nvarchar(20) NOT NULL,
        [RequiredHeroCount] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_xtMissions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_xtMissions_Difficulty] CHECK ([Difficulty] IN (N'Easy', N'Medium', N'Hard')),
        CONSTRAINT [CK_xtMissions_RequiredHeroCount] CHECK ([RequiredHeroCount] > 0),
        CONSTRAINT [CK_xtMissions_Status] CHECK ([Status] IN (N'Pending', N'InProgress', N'Success', N'Failed'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtPowers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_xtPowers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtRoles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_xtRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtSuperheroes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [RealName] nvarchar(150) NULL,
        [Universe] nvarchar(50) NOT NULL,
        [Alignment] nvarchar(20) NOT NULL,
        [Description] nvarchar(max) NULL,
        [PowerLevel] int NOT NULL,
        [Intelligence] int NOT NULL,
        [Strength] int NOT NULL,
        [Speed] int NOT NULL,
        [Durability] int NOT NULL,
        [Combat] int NOT NULL,
        [ImageUrl] nvarchar(500) NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        [UpdatedAt] datetime2(3) NULL,
        CONSTRAINT [PK_xtSuperheroes] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_xtSuperheroes_Alignment] CHECK ([Alignment] IN (N'Hero', N'Villain', N'Anti-Hero')),
        CONSTRAINT [CK_xtSuperheroes_Combat] CHECK ([Combat] BETWEEN 0 AND 100),
        CONSTRAINT [CK_xtSuperheroes_Durability] CHECK ([Durability] BETWEEN 0 AND 100),
        CONSTRAINT [CK_xtSuperheroes_Intelligence] CHECK ([Intelligence] BETWEEN 0 AND 100),
        CONSTRAINT [CK_xtSuperheroes_PowerLevel] CHECK ([PowerLevel] BETWEEN 0 AND 100),
        CONSTRAINT [CK_xtSuperheroes_Speed] CHECK ([Speed] BETWEEN 0 AND 100),
        CONSTRAINT [CK_xtSuperheroes_Strength] CHECK ([Strength] BETWEEN 0 AND 100)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtTeams] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Universe] nvarchar(50) NOT NULL,
        [Description] nvarchar(max) NULL,
        [FoundedDate] date NULL,
        CONSTRAINT [PK_xtTeams] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtUsers] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_xtUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtBattles] (
        [Id] int NOT NULL IDENTITY,
        [Hero1Id] int NOT NULL,
        [Hero2Id] int NOT NULL,
        [WinnerId] int NULL,
        [Hero1Score] int NOT NULL,
        [Hero2Score] int NOT NULL,
        [BattleDate] datetime2(3) NOT NULL,
        CONSTRAINT [PK_xtBattles] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_xtBattles_DistinctHeroes] CHECK ([Hero1Id] <> [Hero2Id]),
        CONSTRAINT [FK_xtBattles_Hero1] FOREIGN KEY ([Hero1Id]) REFERENCES [xtSuperheroes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_xtBattles_Hero2] FOREIGN KEY ([Hero2Id]) REFERENCES [xtSuperheroes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_xtBattles_Winner] FOREIGN KEY ([WinnerId]) REFERENCES [xtSuperheroes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtMissionHeroes] (
        [MissionId] int NOT NULL,
        [SuperheroId] int NOT NULL,
        CONSTRAINT [PK_xtMissionHeroes] PRIMARY KEY ([MissionId], [SuperheroId]),
        CONSTRAINT [FK_xtMissionHeroes_xtMissions] FOREIGN KEY ([MissionId]) REFERENCES [xtMissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_xtMissionHeroes_xtSuperheroes] FOREIGN KEY ([SuperheroId]) REFERENCES [xtSuperheroes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtSuperheroPowers] (
        [SuperheroId] int NOT NULL,
        [PowerId] int NOT NULL,
        CONSTRAINT [PK_xtSuperheroPowers] PRIMARY KEY ([SuperheroId], [PowerId]),
        CONSTRAINT [FK_xtSuperheroPowers_xtPowers] FOREIGN KEY ([PowerId]) REFERENCES [xtPowers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_xtSuperheroPowers_xtSuperheroes] FOREIGN KEY ([SuperheroId]) REFERENCES [xtSuperheroes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtSuperheroTeams] (
        [SuperheroId] int NOT NULL,
        [TeamId] int NOT NULL,
        [JoinedDate] date NOT NULL,
        CONSTRAINT [PK_xtSuperheroTeams] PRIMARY KEY ([SuperheroId], [TeamId]),
        CONSTRAINT [FK_xtSuperheroTeams_xtSuperheroes] FOREIGN KEY ([SuperheroId]) REFERENCES [xtSuperheroes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_xtSuperheroTeams_xtTeams] FOREIGN KEY ([TeamId]) REFERENCES [xtTeams] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE TABLE [xtUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_xtUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_xtUserRoles_xtRoles] FOREIGN KEY ([RoleId]) REFERENCES [xtRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_xtUserRoles_xtUsers] FOREIGN KEY ([UserId]) REFERENCES [xtUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtBattles_BattleDate] ON [xtBattles] ([BattleDate] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtBattles_Hero1Id] ON [xtBattles] ([Hero1Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtBattles_Hero2Id] ON [xtBattles] ([Hero2Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtBattles_WinnerId] ON [xtBattles] ([WinnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtMissionHeroes_SuperheroId] ON [xtMissionHeroes] ([SuperheroId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtMissions_Status] ON [xtMissions] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_xtPowers_Name] ON [xtPowers] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_xtRoles_Name] ON [xtRoles] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtSuperheroes_Name] ON [xtSuperheroes] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtSuperheroes_PowerLevel] ON [xtSuperheroes] ([PowerLevel] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtSuperheroes_Universe_Alignment] ON [xtSuperheroes] ([Universe], [Alignment]) INCLUDE ([PowerLevel]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtSuperheroPowers_PowerId] ON [xtSuperheroPowers] ([PowerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtSuperheroTeams_TeamId] ON [xtSuperheroTeams] ([TeamId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtTeams_Universe] ON [xtTeams] ([Universe]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_xtTeams_Name] ON [xtTeams] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_xtUserRoles_RoleId] ON [xtUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_xtUsers_Email] ON [xtUsers] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_xtUsers_Username] ON [xtUsers] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815105807_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260815105807_InitialCreate', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815111652_AddRefreshTokens'
)
BEGIN
    CREATE TABLE [xtRefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Token] nvarchar(200) NOT NULL,
        [ExpiresAt] datetime2(3) NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        [RevokedAt] datetime2(3) NULL,
        [ReplacedByToken] nvarchar(200) NULL,
        CONSTRAINT [PK_xtRefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_xtRefreshTokens_xtUsers] FOREIGN KEY ([UserId]) REFERENCES [xtUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815111652_AddRefreshTokens'
)
BEGIN
    CREATE INDEX [IX_xtRefreshTokens_UserId] ON [xtRefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815111652_AddRefreshTokens'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_xtRefreshTokens_Token] ON [xtRefreshTokens] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815111652_AddRefreshTokens'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260815111652_AddRefreshTokens', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[xtUsers]') AND [c].[name] = N'IsActive');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [xtUsers] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [xtUsers] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[xtUsers]') AND [c].[name] = N'CreatedAt');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [xtUsers] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [xtUsers] ADD DEFAULT (SYSUTCDATETIME()) FOR [CreatedAt];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[xtSuperheroTeams]') AND [c].[name] = N'JoinedDate');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [xtSuperheroTeams] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [xtSuperheroTeams] ADD DEFAULT (CAST(SYSUTCDATETIME() AS DATE)) FOR [JoinedDate];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[xtSuperheroes]') AND [c].[name] = N'CreatedAt');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [xtSuperheroes] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [xtSuperheroes] ADD DEFAULT (SYSUTCDATETIME()) FOR [CreatedAt];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[xtMissions]') AND [c].[name] = N'Status');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [xtMissions] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [xtMissions] ADD DEFAULT N'Pending' FOR [Status];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[xtMissions]') AND [c].[name] = N'CreatedAt');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [xtMissions] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [xtMissions] ADD DEFAULT (SYSUTCDATETIME()) FOR [CreatedAt];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[xtBattles]') AND [c].[name] = N'BattleDate');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [xtBattles] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [xtBattles] ADD DEFAULT (SYSUTCDATETIME()) FOR [BattleDate];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815124800_AddColumnDefaults'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260815124800_AddColumnDefaults', N'10.0.11');
END;

COMMIT;
GO

