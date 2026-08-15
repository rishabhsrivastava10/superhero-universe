-- Runs every table script in dependency order (parents before children).
-- Run via: sqlcmd -S <server> -E -i 12_RunAllTables.sql
-- (or, in SSMS, enable Query > SQLCMD Mode first, then execute this file)
:r .\00_CreateDatabase.sql
:r .\01_xtUsers.sql
:r .\02_xtRoles.sql
:r .\03_xtUserRoles.sql
:r .\04_xtSuperheroes.sql
:r .\05_xtPowers.sql
:r .\06_xtSuperheroPowers.sql
:r .\07_xtTeams.sql
:r .\08_xtSuperheroTeams.sql
:r .\09_xtMissions.sql
:r .\10_xtMissionHeroes.sql
:r .\11_xtBattles.sql
PRINT N'All SuperheroUniverse tables created successfully.';
GO
