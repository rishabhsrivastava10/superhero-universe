USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtUserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtUserRoles
    (
        UserId INT NOT NULL,
        RoleId INT NOT NULL,

        CONSTRAINT PK_xtUserRoles PRIMARY KEY CLUSTERED (UserId, RoleId),
        CONSTRAINT FK_xtUserRoles_xtUsers FOREIGN KEY (UserId) REFERENCES dbo.xtUsers (Id) ON DELETE CASCADE,
        CONSTRAINT FK_xtUserRoles_xtRoles FOREIGN KEY (RoleId) REFERENCES dbo.xtRoles (Id) ON DELETE CASCADE
    );
END
GO

-- PK covers (UserId, RoleId); a separate index on RoleId speeds up "which users have this role" lookups.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtUserRoles_RoleId')
    CREATE NONCLUSTERED INDEX IX_xtUserRoles_RoleId ON dbo.xtUserRoles (RoleId);
GO
