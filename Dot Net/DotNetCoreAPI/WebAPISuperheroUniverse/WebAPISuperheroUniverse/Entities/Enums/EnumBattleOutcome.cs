namespace WebAPISuperheroUniverse.Entities.Enums;

public enum EnumBattleOutcome
{
    Success,

    /// <summary>One or both of the supplied superhero ids does not exist.</summary>
    HeroNotFound,

    /// <summary>Both ids were the same. Mirrors the CK_xtBattles_DistinctHeroes constraint.</summary>
    SameHero,
}
