USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtSuperheroPowers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtSuperheroPowers
    (
        SuperheroId INT NOT NULL,
        PowerId     INT NOT NULL,

        CONSTRAINT PK_xtSuperheroPowers PRIMARY KEY CLUSTERED (SuperheroId, PowerId),
        CONSTRAINT FK_xtSuperheroPowers_xtSuperheroes FOREIGN KEY (SuperheroId) REFERENCES dbo.xtSuperheroes (Id) ON DELETE CASCADE,
        CONSTRAINT FK_xtSuperheroPowers_xtPowers       FOREIGN KEY (PowerId)     REFERENCES dbo.xtPowers (Id)     ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtSuperheroPowers_PowerId')
    CREATE NONCLUSTERED INDEX IX_xtSuperheroPowers_PowerId ON dbo.xtSuperheroPowers (PowerId);
GO
