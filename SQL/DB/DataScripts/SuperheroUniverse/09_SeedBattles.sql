USE SuperheroUniverseDb;
GO

-- One illustrative historical battle so the "battle history" screen has data to show
-- before the real simulator (Phase 8) starts generating battles.
IF NOT EXISTS (SELECT 1 FROM dbo.xtBattles WHERE Hero1Score = 95 AND Hero2Score = 90)
BEGIN
    DECLARE @SupermanId INT = (SELECT Id FROM dbo.xtSuperheroes WHERE Name = N'Superman');
    DECLARE @ThorId     INT = (SELECT Id FROM dbo.xtSuperheroes WHERE Name = N'Thor');

    IF @SupermanId IS NOT NULL AND @ThorId IS NOT NULL
    BEGIN
        INSERT INTO dbo.xtBattles (Hero1Id, Hero2Id, WinnerId, Hero1Score, Hero2Score, BattleDate)
        VALUES (@SupermanId, @ThorId, @SupermanId, 95, 90, DATEADD(DAY, -7, SYSUTCDATETIME()));
    END
END
GO
