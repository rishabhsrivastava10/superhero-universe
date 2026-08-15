-- Powers and team memberships for the expanded roster added by 13_SeedMoreCharacters.sql.
-- Adds any powers referenced here that don't exist yet, then wires up the relationships.
USE SuperheroUniverseDb;
GO

-- ---------------------------------------------------------------------------------------------
-- Extra powers the wider roster needs
-- ---------------------------------------------------------------------------------------------
DECLARE @NewPowers TABLE (Name NVARCHAR(100), Description NVARCHAR(MAX));
INSERT INTO @NewPowers VALUES
    (N'Stealth',              N'Moving and operating undetected.'),
    (N'Espionage',            N'Intelligence gathering, infiltration and tradecraft.'),
    (N'Shapeshifting',        N'Altering one''s own form or appearance at will.'),
    (N'Illusion Casting',     N'Creating convincing sensory illusions.'),
    (N'Reality Warping',      N'Altering the fabric of reality itself.'),
    (N'Magnetism Control',    N'Generating and manipulating magnetic fields.'),
    (N'Hydrokinesis',         N'Controlling water and commanding sea life.'),
    (N'Hard-Light Constructs',N'Forming solid objects from focused energy and willpower.'),
    (N'Technopathy',          N'Interfacing with and controlling computer systems directly.'),
    (N'Acrobatics',           N'Elite gymnastic and aerial movement skill.'),
    (N'Tactical Genius',      N'Exceptional strategic and battlefield planning.'),
    (N'Cosmic Energy',        N'Channelling cosmic-level destructive force.');

INSERT INTO dbo.xtPowers (Name, Description)
SELECT n.Name, n.Description
FROM @NewPowers n
WHERE NOT EXISTS (SELECT 1 FROM dbo.xtPowers p WHERE p.Name = n.Name);
GO

-- ---------------------------------------------------------------------------------------------
-- Hero -> power assignments
-- ---------------------------------------------------------------------------------------------
DECLARE @Map TABLE (HeroName NVARCHAR(100), PowerName NVARCHAR(100));
INSERT INTO @Map VALUES
    (N'Black Widow',    N'Stealth'),               (N'Black Widow',    N'Espionage'),          (N'Black Widow',    N'Martial Arts Mastery'), (N'Black Widow', N'Agility'),
    (N'Doctor Strange', N'Magic'),                 (N'Doctor Strange', N'Illusion Casting'),   (N'Doctor Strange', N'Flight'),               (N'Doctor Strange', N'Genius-Level Intellect'),
    (N'Black Panther',  N'Super Strength'),        (N'Black Panther',  N'Agility'),            (N'Black Panther',  N'Martial Arts Mastery'), (N'Black Panther', N'Tactical Genius'),
    (N'Wolverine',      N'Healing Factor'),        (N'Wolverine',      N'Super Strength'),     (N'Wolverine',      N'Martial Arts Mastery'),
    (N'Deadpool',       N'Healing Factor'),        (N'Deadpool',       N'Weapons Mastery'),    (N'Deadpool',       N'Martial Arts Mastery'), (N'Deadpool', N'Agility'),
    (N'Scarlet Witch',  N'Reality Warping'),       (N'Scarlet Witch',  N'Magic'),              (N'Scarlet Witch',  N'Telepathy'),            (N'Scarlet Witch', N'Energy Projection'),
    (N'Loki',           N'Shapeshifting'),         (N'Loki',           N'Illusion Casting'),   (N'Loki',           N'Magic'),                (N'Loki', N'Genius-Level Intellect'),
    (N'Thanos',         N'Super Strength'),        (N'Thanos',         N'Invulnerability'),    (N'Thanos',         N'Cosmic Energy'),        (N'Thanos', N'Tactical Genius'),
    (N'Magneto',        N'Magnetism Control'),     (N'Magneto',        N'Flight'),             (N'Magneto',        N'Genius-Level Intellect'),
    (N'Aquaman',        N'Hydrokinesis'),          (N'Aquaman',        N'Super Strength'),     (N'Aquaman',        N'Invulnerability'),      (N'Aquaman', N'Weapons Mastery'),
    (N'Green Lantern',  N'Hard-Light Constructs'), (N'Green Lantern',  N'Flight'),             (N'Green Lantern',  N'Energy Projection'),
    (N'Cyborg',         N'Technopathy'),           (N'Cyborg',         N'Super Strength'),     (N'Cyborg',         N'Energy Projection'),    (N'Cyborg', N'Genius-Level Intellect'),
    (N'Nightwing',      N'Acrobatics'),            (N'Nightwing',      N'Martial Arts Mastery'), (N'Nightwing',    N'Weapons Mastery'),      (N'Nightwing', N'Tactical Genius'),
    (N'Catwoman',       N'Stealth'),               (N'Catwoman',       N'Acrobatics'),         (N'Catwoman',       N'Agility'),
    (N'Lex Luthor',     N'Genius-Level Intellect'),(N'Lex Luthor',     N'Tactical Genius'),
    (N'Harley Quinn',   N'Acrobatics'),            (N'Harley Quinn',   N'Weapons Mastery'),    (N'Harley Quinn',   N'Agility'),
    (N'Darkseid',       N'Cosmic Energy'),         (N'Darkseid',       N'Super Strength'),     (N'Darkseid',       N'Invulnerability'),      (N'Darkseid', N'Energy Projection'),
    (N'Reverse-Flash',  N'Super Speed'),           (N'Reverse-Flash',  N'Healing Factor'),     (N'Reverse-Flash',  N'Genius-Level Intellect');

INSERT INTO dbo.xtSuperheroPowers (SuperheroId, PowerId)
SELECT h.Id, p.Id
FROM @Map m
JOIN dbo.xtSuperheroes h ON h.Name = m.HeroName
JOIN dbo.xtPowers p ON p.Name = m.PowerName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.xtSuperheroPowers x WHERE x.SuperheroId = h.Id AND x.PowerId = p.Id
);
GO

-- ---------------------------------------------------------------------------------------------
-- New teams + memberships
-- ---------------------------------------------------------------------------------------------
DECLARE @Teams TABLE (Name NVARCHAR(100), Universe NVARCHAR(50), Description NVARCHAR(MAX), FoundedDate DATE);
INSERT INTO @Teams VALUES
    (N'Sinister Six',    N'Marvel', N'A shifting alliance of Marvel''s most dangerous villains, united only by a common enemy.', '1964-01-01'),
    (N'Legion of Doom',  N'DC',     N'DC''s premier villain coalition, assembled to counter the Justice League.',                '1978-09-01'),
    (N'Birds of Prey',   N'DC',     N'A covert team operating in the shadows of Gotham and beyond.',                            '1996-01-01');

INSERT INTO dbo.xtTeams (Name, Universe, Description, FoundedDate)
SELECT t.Name, t.Universe, t.Description, t.FoundedDate
FROM @Teams t
WHERE NOT EXISTS (SELECT 1 FROM dbo.xtTeams x WHERE x.Name = t.Name);
GO

DECLARE @TeamMap TABLE (HeroName NVARCHAR(100), TeamName NVARCHAR(100));
INSERT INTO @TeamMap VALUES
    -- Existing teams gain new members
    (N'Black Widow',    N'Avengers'),        (N'Doctor Strange', N'Avengers'),      (N'Black Panther', N'Avengers'),
    (N'Scarlet Witch',  N'Avengers'),
    (N'Aquaman',        N'Justice League'),  (N'Green Lantern',  N'Justice League'), (N'Cyborg',       N'Justice League'),
    (N'Wolverine',      N'X-Men'),           (N'Magneto',        N'X-Men'),
    (N'Nightwing',      N'Teen Titans'),     (N'Cyborg',         N'Teen Titans'),
    -- Villain rosters
    (N'Loki',           N'Sinister Six'),    (N'Thanos',         N'Sinister Six'),
    (N'Joker',          N'Legion of Doom'),  (N'Lex Luthor',     N'Legion of Doom'), (N'Darkseid',     N'Legion of Doom'),
    (N'Reverse-Flash',  N'Legion of Doom'),
    (N'Catwoman',       N'Birds of Prey'),   (N'Harley Quinn',   N'Birds of Prey');

INSERT INTO dbo.xtSuperheroTeams (SuperheroId, TeamId)
SELECT h.Id, t.Id
FROM @TeamMap m
JOIN dbo.xtSuperheroes h ON h.Name = m.HeroName
JOIN dbo.xtTeams t ON t.Name = m.TeamName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.xtSuperheroTeams x WHERE x.SuperheroId = h.Id AND x.TeamId = t.Id
);
GO
