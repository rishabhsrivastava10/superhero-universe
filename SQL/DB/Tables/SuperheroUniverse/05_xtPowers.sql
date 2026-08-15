USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtPowers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtPowers
    (
        Id          INT           IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(100) NOT NULL,
        Description NVARCHAR(MAX) NULL,

        CONSTRAINT PK_xtPowers PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_xtPowers_Name UNIQUE (Name)
    );
END
GO
