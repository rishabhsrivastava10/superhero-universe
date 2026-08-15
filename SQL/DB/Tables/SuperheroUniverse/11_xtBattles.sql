USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtBattles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtBattles
    (
        Id         INT          IDENTITY(1,1) NOT NULL,
        Hero1Id    INT          NOT NULL,
        Hero2Id    INT          NOT NULL,
        WinnerId   INT          NULL,
        Hero1Score INT          NOT NULL,
        Hero2Score INT          NOT NULL,
        BattleDate DATETIME2(3) NOT NULL CONSTRAINT DF_xtBattles_BattleDate DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_xtBattles PRIMARY KEY CLUSTERED (Id),

        -- Business rule: a hero cannot battle itself.
        CONSTRAINT CK_xtBattles_DistinctHeroes CHECK (Hero1Id <> Hero2Id),

        -- All three FKs point back to xtSuperheroes. SQL Server refuses ON DELETE CASCADE here
        -- because more than one cascade path would converge on the same parent/child pair -
        -- deleting a hero must instead be handled deliberately in the service layer (e.g. block
        -- deletion if the hero has battle history, or soft-delete instead of a hard delete).
        CONSTRAINT FK_xtBattles_Hero1  FOREIGN KEY (Hero1Id)  REFERENCES dbo.xtSuperheroes (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_xtBattles_Hero2  FOREIGN KEY (Hero2Id)  REFERENCES dbo.xtSuperheroes (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_xtBattles_Winner FOREIGN KEY (WinnerId) REFERENCES dbo.xtSuperheroes (Id) ON DELETE NO ACTION
    );
END
GO

-- Supports "recent battles" queries (dashboard, battle history).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtBattles_BattleDate')
    CREATE NONCLUSTERED INDEX IX_xtBattles_BattleDate ON dbo.xtBattles (BattleDate DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtBattles_Hero1Id')
    CREATE NONCLUSTERED INDEX IX_xtBattles_Hero1Id ON dbo.xtBattles (Hero1Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtBattles_Hero2Id')
    CREATE NONCLUSTERED INDEX IX_xtBattles_Hero2Id ON dbo.xtBattles (Hero2Id);
GO
