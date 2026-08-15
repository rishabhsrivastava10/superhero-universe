-- Runs every seed script in dependency order.
-- Run via: sqlcmd -S <server> -E -i 10_RunAllSeed.sql
-- (or, in SSMS, enable Query > SQLCMD Mode first, then execute this file)
:r .\01_SeedRoles.sql
:r .\02_SeedPowers.sql
:r .\03_SeedSuperheroes.sql
:r .\04_SeedSuperheroPowers.sql
:r .\05_SeedTeams.sql
:r .\06_SeedSuperheroTeams.sql
:r .\07_SeedMissions.sql
:r .\08_SeedMissionHeroes.sql
:r .\09_SeedBattles.sql
PRINT N'All SuperheroUniverse seed data applied successfully.';
GO
