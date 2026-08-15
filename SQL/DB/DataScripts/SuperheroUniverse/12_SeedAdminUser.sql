-- Seeds a DEVELOPMENT administrator account.
--
-- Username: admin
-- Password: Admin@12345
--
-- The stored value is a real BCrypt hash (work factor 12) of that password - the plain text is
-- never stored, and is written here only because this is a throwaway local development account.
--
-- !! DO NOT run this against any shared or production database, and change the password before
-- !! exposing this API anywhere. Admin is deliberately NOT grantable via self-registration
-- !! (AuthService always assigns the User role), so the first admin has to be created here.
USE SuperheroUniverseDb;
GO

DECLARE @AdminRoleId INT = (SELECT Id FROM dbo.xtRoles WHERE Name = N'Admin');
DECLARE @UserRoleId  INT = (SELECT Id FROM dbo.xtRoles WHERE Name = N'User');

IF @AdminRoleId IS NULL OR @UserRoleId IS NULL
BEGIN
    RAISERROR (N'Roles are not seeded. Run 01_SeedRoles.sql first.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM dbo.xtUsers WHERE Username = N'admin')
BEGIN
    INSERT INTO dbo.xtUsers (Username, Email, PasswordHash, CreatedAt, IsActive)
    VALUES (N'admin', N'admin@superherouniverse.local',
            N'$2a$12$NnFpH.PTKEtmpRCX5ZmhCeMnz1LEuSf3xsu8NrPiRJKJ9G25QB9SK',
            SYSUTCDATETIME(), 1);
END

DECLARE @AdminUserId INT = (SELECT Id FROM dbo.xtUsers WHERE Username = N'admin');

IF NOT EXISTS (SELECT 1 FROM dbo.xtUserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
    INSERT INTO dbo.xtUserRoles (UserId, RoleId) VALUES (@AdminUserId, @AdminRoleId);

-- Admins are users too - granting both keeps role checks simple.
IF NOT EXISTS (SELECT 1 FROM dbo.xtUserRoles WHERE UserId = @AdminUserId AND RoleId = @UserRoleId)
    INSERT INTO dbo.xtUserRoles (UserId, RoleId) VALUES (@AdminUserId, @UserRoleId);
GO
