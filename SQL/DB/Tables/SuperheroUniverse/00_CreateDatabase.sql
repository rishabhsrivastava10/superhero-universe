-- Creates the SuperheroUniverseDb database if it doesn't already exist.
IF DB_ID(N'SuperheroUniverseDb') IS NULL
BEGIN
    CREATE DATABASE SuperheroUniverseDb;
END
GO
