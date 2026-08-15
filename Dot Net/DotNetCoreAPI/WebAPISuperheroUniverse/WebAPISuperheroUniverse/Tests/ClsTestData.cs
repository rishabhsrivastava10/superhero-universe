using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.Tests;

/// <summary>
/// Builders for the entities the calculators operate on. Keeping construction here means a test
/// states only the attributes it actually cares about, so the intent of each case stays visible.
/// </summary>
internal static class ClsTestData
{
    public static ModelSuperhero Hero(
        int id = 1,
        string name = "Test Hero",
        int powerLevel = 50,
        int strength = 50,
        int combat = 50,
        int speed = 50,
        int durability = 50,
        int intelligence = 50,
        string universe = "Marvel",
        string alignment = "Hero") =>
        new()
        {
            Id = id,
            Name = name,
            Universe = universe,
            Alignment = alignment,
            PowerLevel = powerLevel,
            Strength = strength,
            Combat = combat,
            Speed = speed,
            Durability = durability,
            Intelligence = intelligence,
        };

    /// <summary>A hero with every attribute set to the same value - useful for weighting maths.</summary>
    public static ModelSuperhero FlatHero(int id, string name, int allAttributes) =>
        Hero(id, name, allAttributes, allAttributes, allAttributes, allAttributes, allAttributes, allAttributes);

    public static ModelMission Mission(
        int id = 1,
        string title = "Test Mission",
        string difficulty = "Medium",
        int requiredHeroCount = 3) =>
        new()
        {
            Id = id,
            Title = title,
            Difficulty = difficulty,
            RequiredHeroCount = requiredHeroCount,
            Status = "Pending",
        };
}
