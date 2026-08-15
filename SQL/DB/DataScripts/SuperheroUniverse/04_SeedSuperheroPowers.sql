USE SuperheroUniverseDb;
GO

DECLARE @Map TABLE (HeroName NVARCHAR(100), PowerName NVARCHAR(100));
INSERT INTO @Map (HeroName, PowerName) VALUES
    (N'Batman',         N'Genius-Level Intellect'), (N'Batman',         N'Weapons Mastery'),   (N'Batman',         N'Martial Arts Mastery'),
    (N'Superman',       N'Super Strength'),         (N'Superman',       N'Flight'),            (N'Superman',       N'Heat Vision'), (N'Superman', N'Super Speed'), (N'Superman', N'X-Ray Vision'),
    (N'Wonder Woman',   N'Super Strength'),         (N'Wonder Woman',   N'Flight'),            (N'Wonder Woman',   N'Martial Arts Mastery'), (N'Wonder Woman', N'Invulnerability'),
    (N'The Flash',      N'Super Speed'),            (N'The Flash',      N'Healing Factor'),    (N'The Flash',      N'Agility'),
    (N'Joker',          N'Genius-Level Intellect'),
    (N'Spider-Man',     N'Super Strength'),         (N'Spider-Man',     N'Wall-Crawling'),     (N'Spider-Man',     N'Spider-Sense'), (N'Spider-Man', N'Agility'),
    (N'Iron Man',       N'Genius-Level Intellect'), (N'Iron Man',       N'Flight'),            (N'Iron Man',       N'Energy Projection'), (N'Iron Man', N'Weapons Mastery'),
    (N'Thor',           N'Super Strength'),         (N'Thor',           N'Flight'),            (N'Thor',           N'Magic'), (N'Thor', N'Weapons Mastery'), (N'Thor', N'Invulnerability'),
    (N'Hulk',           N'Super Strength'),         (N'Hulk',           N'Invulnerability'),   (N'Hulk',           N'Healing Factor'),
    (N'Captain Marvel', N'Super Strength'),         (N'Captain Marvel', N'Flight'),            (N'Captain Marvel', N'Energy Projection'), (N'Captain Marvel', N'Invulnerability'), (N'Captain Marvel', N'Super Speed');

INSERT INTO dbo.xtSuperheroPowers (SuperheroId, PowerId)
SELECT h.Id, p.Id
FROM @Map m
JOIN dbo.xtSuperheroes h ON h.Name = m.HeroName
JOIN dbo.xtPowers p ON p.Name = m.PowerName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.xtSuperheroPowers x WHERE x.SuperheroId = h.Id AND x.PowerId = p.Id
);
GO
