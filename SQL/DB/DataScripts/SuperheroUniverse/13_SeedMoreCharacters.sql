-- Adds a wider roster of heroes, villains and anti-heroes.
--
-- IMAGES
-- ------
-- This script deliberately leaves ImageUrl NULL. Portraits are applied separately by
-- 15_SeedCharacterArtwork.sql, and any character without one falls back to their initials in
-- the UI.
--
-- An earlier version generated DiceBear avatars here so every character had *something*. That
-- was worse than nothing: a randomly generated cartoon face bears no resemblance to the
-- character, so it actively misinforms - Green Lantern rendered as a smiling woman with flowers
-- in her hair. Initials are honest about the fact that no portrait exists.
USE SuperheroUniverseDb;
GO

DECLARE @Roster TABLE
(
    Name NVARCHAR(100), RealName NVARCHAR(150), Universe NVARCHAR(50), Alignment NVARCHAR(20),
    Description NVARCHAR(MAX), PowerLevel INT, Intelligence INT, Strength INT, Speed INT,
    Durability INT, Combat INT
);

-- ---------------------------------------------------------------------------------------------
-- Marvel
-- ---------------------------------------------------------------------------------------------
INSERT INTO @Roster VALUES
    (N'Black Widow',   N'Natasha Romanoff', N'Marvel', N'Hero',      N'A former spy and master assassin whose skill, tradecraft and resolve let her stand beside gods.',                          72, 88, 45, 60, 50, 95),
    (N'Doctor Strange',N'Stephen Strange',  N'Marvel', N'Hero',      N'A brilliant surgeon turned Sorcerer Supreme, defending reality from mystical threats.',                                  93, 96, 45, 60, 65, 80),
    (N'Black Panther', N'T''Challa',        N'Marvel', N'Hero',      N'King of Wakanda, combining a vibranium suit, peak conditioning and formidable strategic intellect.',                     85, 92, 70, 75, 80, 92),
    (N'Wolverine',     N'Logan',            N'Marvel', N'Anti-Hero', N'A mutant with a regenerative healing factor, adamantium skeleton and a long, violent memory.',                           83, 65, 78, 65, 92, 96),
    (N'Deadpool',      N'Wade Wilson',      N'Marvel', N'Anti-Hero', N'A mercenary whose accelerated healing makes him nearly impossible to kill, and impossible to quieten.',                  76, 70, 60, 62, 95, 88),
    (N'Scarlet Witch', N'Wanda Maximoff',   N'Marvel', N'Anti-Hero', N'A reality-warping mutant whose chaos magic makes her one of the most dangerous beings alive.',                           96, 85, 50, 65, 60, 70),
    (N'Loki',          N'Loki Laufeyson',   N'Marvel', N'Villain',   N'The God of Mischief - an Asgardian illusionist and schemer who rarely fights fair.',                                     84, 95, 70, 70, 85, 78),
    (N'Thanos',        N'Thanos',           N'Marvel', N'Villain',   N'A Titan warlord of overwhelming strength and conviction, willing to remake the universe by force.',                      99, 92, 100, 75, 100, 90),
    (N'Magneto',       N'Erik Lehnsherr',   N'Marvel', N'Villain',   N'A mutant master of magnetism whose control over metal makes entire armies irrelevant.',                                  91, 94, 60, 65, 75, 76);

-- ---------------------------------------------------------------------------------------------
-- DC
-- ---------------------------------------------------------------------------------------------
INSERT INTO @Roster VALUES
    (N'Aquaman',       N'Arthur Curry',     N'DC',     N'Hero',      N'King of Atlantis, commanding the seas with superhuman strength and durability.',                                         88, 75, 92, 70, 90, 82),
    (N'Green Lantern', N'Hal Jordan',       N'DC',     N'Hero',      N'A test pilot chosen by a power ring that turns willpower into hard light constructs.',                                   89, 80, 70, 80, 75, 78),
    (N'Cyborg',        N'Victor Stone',     N'DC',     N'Hero',      N'Half man, half machine - an athlete rebuilt with technology that interfaces with any system.',                           82, 90, 80, 70, 88, 74),
    (N'Nightwing',     N'Dick Grayson',     N'DC',     N'Hero',      N'The first Robin, now a leader in his own right and one of the finest acrobats and fighters alive.',                       74, 88, 55, 68, 58, 93),
    (N'Catwoman',      N'Selina Kyle',      N'DC',     N'Anti-Hero', N'A master thief whose loyalties shift, but whose skill and agility never do.',                                            68, 82, 42, 62, 48, 86),
    (N'Lex Luthor',    N'Alexander Luthor', N'DC',     N'Villain',   N'A ruthless industrialist whose greatest weapon is an intellect that needs no superpowers.',                              80, 100, 55, 45, 65, 62),
    (N'Harley Quinn',  N'Harleen Quinzel',  N'DC',     N'Anti-Hero', N'A former psychiatrist turned chaotic force, unpredictable and far more capable than she appears.',                        66, 84, 48, 60, 55, 80),
    (N'Darkseid',      N'Uxas',             N'DC',     N'Villain',   N'The tyrant of Apokolips, whose Omega Beams and sheer power rival any being in the universe.',                            98, 93, 100, 80, 100, 88),
    (N'Reverse-Flash', N'Eobard Thawne',    N'DC',     N'Villain',   N'A speedster from the future whose obsession with the Flash drives him to rewrite history itself.',                        87, 88, 50, 99, 62, 74);

-- Insert only the characters that don't already exist, so this script is safe to re-run.
-- ImageUrl is left NULL on purpose - see the note at the top of this file.
INSERT INTO dbo.xtSuperheroes
    (Name, RealName, Universe, Alignment, Description, PowerLevel, Intelligence, Strength, Speed, Durability, Combat)
SELECT r.Name, r.RealName, r.Universe, r.Alignment, r.Description,
       r.PowerLevel, r.Intelligence, r.Strength, r.Speed, r.Durability, r.Combat
FROM @Roster r
WHERE NOT EXISTS (SELECT 1 FROM dbo.xtSuperheroes x WHERE x.Name = r.Name);
GO
