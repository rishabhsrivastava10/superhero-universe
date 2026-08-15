-- Points every character's ImageUrl at real character artwork from the public
-- akabab/superhero-api dataset (served via the jsDelivr CDN).
--
-- !! INTELLECTUAL PROPERTY WARNING - READ BEFORE DEPLOYING PUBLICLY !!
-- ------------------------------------------------------------------
-- These images are Marvel/DC character artwork. The API only re-hosts them; it does not own or
-- license them. That is acceptable for a local, private learning project, but it conflicts with
-- the project brief's rule that the app must not depend on copyrighted assets, and it should NOT
-- be used for anything published, commercial, or shown as a hosted portfolio site.
--
-- To remove all third-party artwork and fall back to initials everywhere, run:
--     UPDATE dbo.xtSuperheroes SET ImageUrl = NULL;
--
-- Nothing in the application depends on this: xtSuperheroes.ImageUrl is just a URL, and the UI
-- falls back to the character's initials whenever it is NULL or the image fails to load.
--
-- Two characters (Green Lantern, Reverse-Flash) are absent from that dataset, so their ImageUrl
-- stays NULL and they render as initials - which is exactly why that fallback matters.
--
-- Character/image ids below were verified against the dataset, not guessed: a name-only search
-- matched Nightwing to Naruto and Darkseid to Data from Star Trek.
USE SuperheroUniverseDb;
GO

UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/69-batman.jpg' WHERE Name = N'Batman';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/644-superman.jpg' WHERE Name = N'Superman';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/720-wonder-woman.jpg' WHERE Name = N'Wonder Woman';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/263-flash.jpg' WHERE Name = N'The Flash';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/370-joker.jpg' WHERE Name = N'Joker';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/620-spider-man.jpg' WHERE Name = N'Spider-Man';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/346-iron-man.jpg' WHERE Name = N'Iron Man';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/659-thor.jpg' WHERE Name = N'Thor';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/332-hulk.jpg' WHERE Name = N'Hulk';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/157-captain-marvel.jpg' WHERE Name = N'Captain Marvel';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/107-black-widow.jpg' WHERE Name = N'Black Widow';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/226-doctor-strange.jpg' WHERE Name = N'Doctor Strange';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/106-black-panther.jpg' WHERE Name = N'Black Panther';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/717-wolverine.jpg' WHERE Name = N'Wolverine';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/213-deadpool.jpg' WHERE Name = N'Deadpool';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/579-scarlet-witch.jpg' WHERE Name = N'Scarlet Witch';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/414-loki.jpg' WHERE Name = N'Loki';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/655-thanos.jpg' WHERE Name = N'Thanos';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/423-magneto.jpg' WHERE Name = N'Magneto';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/38-aquaman.jpg' WHERE Name = N'Aquaman';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/194-cyborg.jpg' WHERE Name = N'Cyborg';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/491-nightwing.jpg' WHERE Name = N'Nightwing';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/165-catwoman.jpg' WHERE Name = N'Catwoman';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/405-lex-luthor.jpg' WHERE Name = N'Lex Luthor';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/309-harley-quinn.jpg' WHERE Name = N'Harley Quinn';
UPDATE dbo.xtSuperheroes SET ImageUrl = N'https://cdn.jsdelivr.net/gh/akabab/superhero-api@0.3.0/api/images/md/204-darkseid.jpg' WHERE Name = N'Darkseid';

GO
