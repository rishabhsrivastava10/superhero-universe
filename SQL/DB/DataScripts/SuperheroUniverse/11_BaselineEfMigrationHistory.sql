-- Baselines an existing SuperheroUniverseDb (one built by the Tables\ scripts) for EF Core.
--
-- Why this exists: the schema was first created by the raw SQL scripts in
-- SQL\DB\Tables\SuperheroUniverse\, then EF Core migrations were introduced afterwards.
-- Running `dotnet ef database update` against such a database would fail, because the
-- migration tries to CREATE TABLE objects that already exist. Recording the migration as
-- already-applied ("baselining") tells EF the database is up to date, so only FUTURE
-- migrations get run against it.
--
-- Only needed for a database created by the raw SQL scripts. A fresh database created by
-- `dotnet ef database update` records this row itself and must NOT be baselined manually.
--
-- The MigrationId must match the migration file name in
-- WebAPISuperheroUniverse\DBContext\EntityFramework\Migrations\ exactly, or EF still
-- treats the migration as pending.
USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory
    (
        MigrationId    NVARCHAR(150) NOT NULL,
        ProductVersion NVARCHAR(32)  NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
    );
END
GO

-- Drop any stale baseline row from a superseded migration (the migration was regenerated when
-- the API was restructured into API/DBContext/Entities projects, which changed its timestamp id).
DELETE FROM dbo.__EFMigrationsHistory
WHERE MigrationId <> N'20260815105807_InitialCreate';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260815105807_InitialCreate')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260815105807_InitialCreate', N'10.0.11');
END
GO
