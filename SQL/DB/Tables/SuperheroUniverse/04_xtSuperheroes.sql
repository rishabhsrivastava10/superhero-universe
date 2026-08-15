USE SuperheroUniverseDb;
GO

IF OBJECT_ID(N'dbo.xtSuperheroes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.xtSuperheroes
    (
        Id           INT           IDENTITY(1,1) NOT NULL,
        Name         NVARCHAR(100) NOT NULL,
        RealName     NVARCHAR(150) NULL,
        Universe     NVARCHAR(50)  NOT NULL,
        Alignment    NVARCHAR(20)  NOT NULL,
        Description  NVARCHAR(MAX) NULL,
        PowerLevel   INT           NOT NULL,
        Intelligence INT           NOT NULL,
        Strength     INT           NOT NULL,
        Speed        INT           NOT NULL,
        Durability   INT           NOT NULL,
        Combat       INT           NOT NULL,
        ImageUrl     NVARCHAR(500) NULL,
        CreatedAt    DATETIME2(3)  NOT NULL CONSTRAINT DF_xtSuperheroes_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2(3)  NULL,

        CONSTRAINT PK_xtSuperheroes PRIMARY KEY CLUSTERED (Id),

        -- Alignment is a fixed, small domain-level enumeration, so it's worth enforcing in the DB.
        -- Universe is deliberately left unconstrained (plain NVARCHAR) so the system can support
        -- original/custom universes later, not just Marvel/DC - validated at the app layer instead.
        CONSTRAINT CK_xtSuperheroes_Alignment    CHECK (Alignment IN (N'Hero', N'Villain', N'Anti-Hero')),
        CONSTRAINT CK_xtSuperheroes_PowerLevel   CHECK (PowerLevel   BETWEEN 0 AND 100),
        CONSTRAINT CK_xtSuperheroes_Intelligence CHECK (Intelligence BETWEEN 0 AND 100),
        CONSTRAINT CK_xtSuperheroes_Strength     CHECK (Strength     BETWEEN 0 AND 100),
        CONSTRAINT CK_xtSuperheroes_Speed        CHECK (Speed        BETWEEN 0 AND 100),
        CONSTRAINT CK_xtSuperheroes_Durability   CHECK (Durability   BETWEEN 0 AND 100),
        CONSTRAINT CK_xtSuperheroes_Combat       CHECK (Combat       BETWEEN 0 AND 100)
    );
END
GO

-- Supports name search (GET /api/superheroes?search=bat).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtSuperheroes_Name')
    CREATE NONCLUSTERED INDEX IX_xtSuperheroes_Name ON dbo.xtSuperheroes (Name);
GO

-- Supports the common filter combo (universe + alignment), with PowerLevel included so a
-- filtered-and-sorted-by-power list can be satisfied straight from the index.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtSuperheroes_Universe_Alignment')
    CREATE NONCLUSTERED INDEX IX_xtSuperheroes_Universe_Alignment ON dbo.xtSuperheroes (Universe, Alignment) INCLUDE (PowerLevel);
GO

-- Supports "sort by power level" / "top N heroes" queries.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_xtSuperheroes_PowerLevel')
    CREATE NONCLUSTERED INDEX IX_xtSuperheroes_PowerLevel ON dbo.xtSuperheroes (PowerLevel DESC);
GO
