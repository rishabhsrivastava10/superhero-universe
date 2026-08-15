USE SuperheroUniverseDb;
GO

DECLARE @Powers TABLE (Name NVARCHAR(100), Description NVARCHAR(MAX));
INSERT INTO @Powers (Name, Description) VALUES
    (N'Super Strength',        N'Physical strength far beyond normal human limits.'),
    (N'Flight',                N'Ability to fly under one''s own power.'),
    (N'Heat Vision',           N'Projecting focused thermal energy beams from the eyes.'),
    (N'Super Speed',           N'Movement and reaction speed far beyond normal human limits.'),
    (N'X-Ray Vision',          N'Ability to see through solid objects.'),
    (N'Wall-Crawling',         N'Ability to adhere to and climb walls/ceilings.'),
    (N'Spider-Sense',          N'A precognitive early-warning sense of nearby danger.'),
    (N'Agility',               N'Exceptional balance, reflexes and body control.'),
    (N'Telepathy',             N'Ability to read or communicate with minds directly.'),
    (N'Invulnerability',       N'Highly resistant or immune to physical harm.'),
    (N'Healing Factor',        N'Accelerated recovery from injury.'),
    (N'Energy Projection',     N'Ability to generate and project destructive energy.'),
    (N'Genius-Level Intellect',N'Intelligence and problem-solving far beyond normal human limits.'),
    (N'Magic',                 N'Command over mystical/supernatural forces.'),
    (N'Weapons Mastery',       N'Expert-level proficiency with weapons and equipment.'),
    (N'Martial Arts Mastery',  N'Expert-level unarmed combat training.');

INSERT INTO dbo.xtPowers (Name, Description)
SELECT p.Name, p.Description
FROM @Powers p
WHERE NOT EXISTS (SELECT 1 FROM dbo.xtPowers x WHERE x.Name = p.Name);
GO
