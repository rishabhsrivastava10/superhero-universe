namespace WebAPISuperheroUniverse.Entities.Enums;

/// <summary>
/// Mirrors the CK_xtSuperheroes_Alignment check constraint. Stored in the database as its
/// string name (not its numeric value), so the DB stays readable and the constraint keeps working.
/// </summary>
public enum EnumAlignment
{
    Hero,
    Villain,
    AntiHero
}

public static class EnumAlignmentExtensions
{
    // The DB value uses a hyphen ("Anti-Hero"), which isn't a legal C# identifier.
    public static string ToDbValue(this EnumAlignment alignment) => alignment switch
    {
        EnumAlignment.Hero => "Hero",
        EnumAlignment.Villain => "Villain",
        EnumAlignment.AntiHero => "Anti-Hero",
        _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "Unknown alignment.")
    };

    public static bool TryParseDbValue(string? value, out EnumAlignment alignment)
    {
        switch (value)
        {
            case "Hero": alignment = EnumAlignment.Hero; return true;
            case "Villain": alignment = EnumAlignment.Villain; return true;
            case "Anti-Hero": alignment = EnumAlignment.AntiHero; return true;
            default: alignment = default; return false;
        }
    }
}
