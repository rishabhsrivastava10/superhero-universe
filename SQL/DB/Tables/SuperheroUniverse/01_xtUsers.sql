USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtUsers
    (
        Id           INT           IDENTITY(1,1) NOT NULL,
        Username     NVARCHAR(50)  NOT NULL,
        Email        NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        CreatedAt    DATETIME2(3)  NOT NULL CONSTRAINT DF_xtUsers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        IsActive     BIT           NOT NULL CONSTRAINT DF_xtUsers_IsActive DEFAULT (1),

        CONSTRAINT PK_xtUsers PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_xtUsers_Username UNIQUE (Username),
        CONSTRAINT UQ_xtUsers_Email UNIQUE (Email)
    );
END
GO
