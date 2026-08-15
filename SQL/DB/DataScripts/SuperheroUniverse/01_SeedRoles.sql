USE SuperheroUniverseDb;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.xtRoles WHERE Name = N'User')
    INSERT INTO dbo.xtRoles (Name) VALUES (N'User');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.xtRoles WHERE Name = N'Admin')
    INSERT INTO dbo.xtRoles (Name) VALUES (N'Admin');
GO
