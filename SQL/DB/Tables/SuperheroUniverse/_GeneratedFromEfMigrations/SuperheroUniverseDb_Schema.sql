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

