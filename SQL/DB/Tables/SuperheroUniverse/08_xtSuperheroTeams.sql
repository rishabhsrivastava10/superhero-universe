USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtSuperheroTeams', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtSuperheroTeams
    (
        SuperheroId INT  NOT NULL,
        TeamId      INT  NOT NULL,
        JoinedDate  DATE NOT NULL CONSTRAINT DF_xtSuperheroTeams_JoinedDate DEFAULT (CAST(SYSUTCDATETIME() AS DATE)),

        CONSTRAINT PK_xtSuperheroTeams PRIMARY KEY CLUSTERED (SuperheroId, TeamId),
        CONSTRAINT FK_xtSuperheroTeams_xtSuperheroes FOREIGN KEY (SuperheroId) REFERENCES dbo.xtSuperheroes (Id) ON DELETE CASCADE,
        CONSTRAINT FK_xtSuperheroTeams_xtTeams       FOREIGN KEY (TeamId)      REFERENCES dbo.xtTeams (Id)      ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtSuperheroTeams_TeamId')
    CREATE NONCLUSTERED INDEX IX_xtSuperheroTeams_TeamId ON dbo.xtSuperheroTeams (TeamId);
GO
