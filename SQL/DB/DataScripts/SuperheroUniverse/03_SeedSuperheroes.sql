USE SuperheroUniverseDb;
GO

DECLARE @Heroes TABLE
(
    Name NVARCHAR(100), RealName NVARCHAR(150), Universe NVARCHAR(50), Alignment NVARCHAR(20),
    Description NVARCHAR(MAX), PowerLevel INT, Intelligence INT, Strength INT, Speed INT, Durability INT, Combat INT
);
INSERT INTO @Heroes (Name, RealName, Universe, Alignment, Description, PowerLevel, Intelligence, Strength, Speed, Durability, Combat) VALUES
    (N'Batman',         N'Bruce Wayne',   N'DC',     N'Hero',   N'A billionaire vigilante who relies on peak physical conditioning, detective skill, and technology rather than superpowers.', 78, 100, 70, 65, 60, 95),
    (N'Superman',       N'Clark Kent',    N'DC',     N'Hero',   N'A Kryptonian with near-invulnerability and a vast array of powers drawn from Earth''s yellow sun.', 98, 90, 100, 100, 100, 85),
    (N'Wonder Woman',   N'Diana Prince',  N'DC',     N'Hero',   N'An Amazonian warrior princess gifted with divine strength and combat mastery.', 95, 85, 95, 90, 95, 95),
    (N'The Flash',      N'Barry Allen',   N'DC',     N'Hero',   N'A forensic scientist granted a connection to the Speed Force.', 85, 80, 55, 100, 65, 70),
    (N'Joker',          N'Unknown',       N'DC',     N'Villain',N'A chaotic criminal mastermind and Batman''s archenemy, relying purely on cunning and unpredictability.', 65, 95, 40, 45, 40, 70),
    (N'Spider-Man',     N'Peter Parker',  N'Marvel', N'Hero',   N'A teenager bitten by a radioactive spider, gaining spider-like abilities and a strong moral code.', 80, 90, 75, 80, 70, 85),
    (N'Iron Man',       N'Tony Stark',    N'Marvel', N'Hero',   N'A genius billionaire engineer who fights crime in a powered exoskeleton suit of his own design.', 88, 100, 85, 85, 85, 80),
    (N'Thor',           N'Thor Odinson',  N'Marvel', N'Hero',   N'The Asgardian God of Thunder, wielding the enchanted hammer Mjolnir.', 97, 75, 100, 90, 100, 90),
    (N'Hulk',           N'Bruce Banner',  N'Marvel', N'Hero',   N'A brilliant physicist who transforms into a being of near-limitless rage-fueled strength.', 92, 60, 100, 60, 100, 80),
    (N'Captain Marvel', N'Carol Danvers', N'Marvel', N'Hero',   N'A former Air Force pilot empowered by Kree technology, among the most powerful heroes on Earth.', 94, 80, 95, 95, 95, 85);

INSERT INTO dbo.xtSuperheroes (Name, RealName, Universe, Alignment, Description, PowerLevel, Intelligence, Strength, Speed, Durability, Combat)
SELECT h.Name, h.RealName, h.Universe, h.Alignment, h.Description, h.PowerLevel, h.Intelligence, h.Strength, h.Speed, h.Durability, h.Combat
FROM @Heroes h
WHERE NOT EXISTS (SELECT 1 FROM dbo.xtSuperheroes x WHERE x.Name = h.Name);
GO
