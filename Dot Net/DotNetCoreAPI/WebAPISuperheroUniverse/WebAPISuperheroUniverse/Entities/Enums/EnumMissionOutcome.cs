namespace WebAPISuperheroUniverse.Entities.Enums;

public enum EnumMissionOutcome
{
    Success,

    /// <summary>The mission id does not exist.</summary>
    MissionNotFound,

    /// <summary>One or more of the supplied superhero ids does not exist.</summary>
    HeroNotFound,

    /// <summary>The mission has already been resolved; it must be reset before another attempt.</summary>
    AlreadyResolved,

    /// <summary>No heroes were supplied for the attempt.</summary>
    NoHeroes,
}
