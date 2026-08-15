USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtMissionHeroes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtMissionHeroes
    (
        MissionId   INT NOT NULL,
        SuperheroId INT NOT NULL,

        CONSTRAINT PK_xtMissionHeroes PRIMARY KEY CLUSTERED (MissionId, SuperheroId),
        CONSTRAINT FK_xtMissionHeroes_xtMissions    FOREIGN KEY (MissionId)   REFERENCES dbo.xtMissions (Id)    ON DELETE CASCADE,
        CONSTRAINT FK_xtMissionHeroes_xtSuperheroes FOREIGN KEY (SuperheroId) REFERENCES dbo.xtSuperheroes (Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtMissionHeroes_SuperheroId')
    CREATE NONCLUSTERED INDEX IX_xtMissionHeroes_SuperheroId ON dbo.xtMissionHeroes (SuperheroId);
GO
