USE SuperheroUniverseDb;
GO

DECLARE @Map TABLE (HeroName NVARCHAR(100), TeamName NVARCHAR(100));
INSERT INTO @Map (HeroName, TeamName) VALUES
    (N'Batman',         N'Justice League'),
    (N'Superman',       N'Justice League'),
    (N'Wonder Woman',   N'Justice League'),
    (N'The Flash',      N'Justice League'),
    (N'Iron Man',       N'Avengers'),
    (N'Thor',           N'Avengers'),
    (N'Hulk',           N'Avengers'),
    (N'Captain Marvel', N'Avengers'),
    (N'Spider-Man',     N'Avengers');

INSERT INTO dbo.xtSuperheroTeams (SuperheroId, TeamId)
SELECT h.Id, t.Id
FROM @Map m
JOIN dbo.xtSuperheroes h ON h.Name = m.HeroName
JOIN dbo.xtTeams t ON t.Name = m.TeamName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.xtSuperheroTeams x WHERE x.SuperheroId = h.Id AND x.TeamId = t.Id
);
GO
