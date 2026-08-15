USE SuperheroUniverseDb;
GO

DECLARE @Missions TABLE (Title NVARCHAR(200), Description NVARCHAR(MAX), Location NVARCHAR(150), Difficulty NVARCHAR(20), RequiredHeroCount INT, Status NVARCHAR(20));
INSERT INTO @Missions (Title, Description, Location, Difficulty, RequiredHeroCount, Status) VALUES
    (N'Stop Hydra Attack',                    N'A Hydra strike team has seized a research facility and must be stopped before they extract its data.', N'New York',      N'Hard',   3, N'Pending'),
    (N'Rescue Hostages from Joker''s Trap',    N'The Joker has rigged a downtown building with hostages inside; a rescue must be coordinated carefully.', N'Gotham City',   N'Medium', 2, N'Pending'),
    (N'Defend Metropolis from Alien Invasion', N'An alien scouting fleet has breached the atmosphere over Metropolis and must be repelled.', N'Metropolis',    N'Hard',   4, N'Success');

INSERT INTO dbo.xtMissions (Title, Description, Location, Difficulty, RequiredHeroCount, Status)
SELECT m.Title, m.Description, m.Location, m.Difficulty, m.RequiredHeroCount, m.Status
FROM @Missions m
WHERE NOT EXISTS (SELECT 1 FROM dbo.xtMissions x WHERE x.Title = m.Title);
GO
