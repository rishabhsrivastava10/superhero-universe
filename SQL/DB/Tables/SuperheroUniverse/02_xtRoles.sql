USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtRoles
    (
        Id   INT          IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(50) NOT NULL,

        CONSTRAINT PK_xtRoles PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_xtRoles_Name UNIQUE (Name)
    );
END
GO
