USE SuperheroUniverseDb;
GO

DECLARE @Teams TABLE (Name NVARCHAR(100), Universe NVARCHAR(50), Description NVARCHAR(MAX), FoundedDate DATE);
INSERT INTO @Teams (Name, Universe, Description, FoundedDate) VALUES
    (N'Justice League', N'DC',     N'Earth''s premier team of DC super heroes, formed to face threats no single hero could handle alone.', '1960-03-01'),
    (N'Avengers',       N'Marvel', N'Earth''s Mightiest Heroes, a Marvel team assembled to fight foes too powerful for any one hero.', '1963-09-01'),
    (N'X-Men',          N'Marvel', N'A team of mutant heroes fighting for peaceful coexistence between mutants and humanity.', '1963-09-01'),
    (N'Guardians of the Galaxy', N'Marvel', N'A ragtag team of cosmic heroes protecting the galaxy from extraterrestrial threats.', '1969-01-01'),
    (N'Teen Titans',    N'DC',     N'A team of young DC heroes operating alongside (and sometimes independently of) the Justice League.', '1964-07-01');

INSERT INTO dbo.xtTeams (Name, Universe, Description, FoundedDate)
SELECT t.Name, t.Universe, t.Description, t.FoundedDate
FROM @Teams t
WHERE NOT EXISTS (SELECT 1 FROM dbo.xtTeams x WHERE x.Name = t.Name);
GO
