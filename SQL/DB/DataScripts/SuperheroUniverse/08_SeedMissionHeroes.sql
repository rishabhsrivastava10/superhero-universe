USE SuperheroUniverseDb;
GO

-- Only the completed mission gets pre-assigned heroes here, as a historical record.
-- Pending missions get their hero assignments through the "start mission" flow (Phase 9).
DECLARE @Map TABLE (MissionTitle NVARCHAR(200), HeroName NVARCHAR(100));
INSERT INTO @Map (MissionTitle, HeroName) VALUES
    (N'Defend Metropolis from Alien Invasion', N'Superman'),
    (N'Defend Metropolis from Alien Invasion', N'Wonder Woman'),
    (N'Defend Metropolis from Alien Invasion', N'The Flash'),
    (N'Defend Metropolis from Alien Invasion', N'Thor');

INSERT INTO dbo.xtMissionHeroes (MissionId, SuperheroId)
SELECT mi.Id, h.Id
FROM @Map m
JOIN dbo.xtMissions mi ON mi.Title = m.MissionTitle
JOIN dbo.xtSuperheroes h ON h.Name = m.HeroName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.xtMissionHeroes x WHERE x.MissionId = mi.Id AND x.SuperheroId = h.Id
);
GO
