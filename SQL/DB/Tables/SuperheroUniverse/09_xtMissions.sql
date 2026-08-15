USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtMissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtMissions
    (
        Id                INT           IDENTITY(1,1) NOT NULL,
        Title             NVARCHAR(200) NOT NULL,
        Description       NVARCHAR(MAX) NULL,
        Location          NVARCHAR(150) NULL,
        Difficulty        NVARCHAR(20)  NOT NULL,
        RequiredHeroCount INT           NOT NULL,
        Status            NVARCHAR(20)  NOT NULL CONSTRAINT DF_xtMissions_Status DEFAULT (N'Pending'),
        CreatedAt         DATETIME2(3)  NOT NULL CONSTRAINT DF_xtMissions_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_xtMissions PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_xtMissions_Difficulty        CHECK (Difficulty IN (N'Easy', N'Medium', N'Hard')),
        CONSTRAINT CK_xtMissions_Status            CHECK (Status IN (N'Pending', N'InProgress', N'Success', N'Failed')),
        CONSTRAINT CK_xtMissions_RequiredHeroCount CHECK (RequiredHeroCount > 0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtMissions_Status')
    CREATE NONCLUSTERED INDEX IX_xtMissions_Status ON dbo.xtMissions (Status);
GO
