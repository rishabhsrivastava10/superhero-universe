USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtTeams', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtTeams
    (
        Id          INT           IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(100) NOT NULL,
        Universe    NVARCHAR(50)  NOT NULL,
        Description NVARCHAR(MAX) NULL,
        FoundedDate DATE          NULL,

        CONSTRAINT PK_xtTeams PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_xtTeams_Name UNIQUE (Name)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtTeams_Universe')
    CREATE NONCLUSTERED INDEX IX_xtTeams_Universe ON dbo.xtTeams (Universe);
GO
