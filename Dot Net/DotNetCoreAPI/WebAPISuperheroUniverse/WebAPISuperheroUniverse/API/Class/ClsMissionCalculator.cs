using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Class;

public sealed record ClsMissionOutcome(
    bool Succeeded,
    int SuccessChancePercent,
    int RollPercent,
    string Summary,
    IReadOnlyList<ModelMissionFactor> Factors);

/// <summary>
/// Decides whether a squad completes a mission.
///
/// Like the battle calculator, this is deliberately free of database and HTTP dependencies so it
/// can be unit tested exhaustively, and it lives on the server because it IS the business rule.
/// </summary>
public sealed class ClsMissionCalculator(IMissionRollProvider rollProvider)
{
    /// <summary>Average squad power level at which a mission is an even 50/50.</summary>
    private static int ParPower(EnumMissionDifficulty difficulty) => difficulty switch
    {
        EnumMissionDifficulty.Easy => 45,
        EnumMissionDifficulty.Medium => 65,
        EnumMissionDifficulty.Hard => 82,
        _ => 65,
    };

    private const int BaseChance = 50;

    /// <summary>Each point of average power above/below par moves the odds by this much.</summary>
    private const double PowerWeight = 1.6;

    /// <summary>Sending fewer heroes than the briefing demands is heavily punished.</summary>
    private const int UnderstaffedPenaltyPerHero = 22;

    /// <summary>Extra bodies help, but with diminishing value - a mission is not won by crowding.</summary>
    private const int OverstaffedBonusPerHero = 5;
    private const int MaxOverstaffedBonus = 15;

    // Never certain either way: even a perfect squad can fail, and a doomed one can get lucky.
    private const int MinChance = 5;
    private const int MaxChance = 95;

    public ClsMissionOutcome Resolve(ModelMission mission, IReadOnlyList<ModelSuperhero> squad)
    {
        ArgumentNullException.ThrowIfNull(mission);
        ArgumentNullException.ThrowIfNull(squad);

        if (squad.Count == 0)
        {
            throw new ArgumentException("A mission attempt needs at least one hero.", nameof(squad));
        }

        var difficulty = Enum.TryParse<EnumMissionDifficulty>(mission.Difficulty, out var parsed)
            ? parsed
            : EnumMissionDifficulty.Medium;

        var par = ParPower(difficulty);

        // Average, not total: otherwise adding any warm body would always help, and a squad of
        // ten weak heroes would beat a hand-picked pair of the strongest.
        var averagePower = squad.Average(h => h.PowerLevel);
        var powerDelta = (int)Math.Round((averagePower - par) * PowerWeight);

        var sizeDelta = squad.Count < mission.RequiredHeroCount
            ? -(mission.RequiredHeroCount - squad.Count) * UnderstaffedPenaltyPerHero
            : Math.Min((squad.Count - mission.RequiredHeroCount) * OverstaffedBonusPerHero, MaxOverstaffedBonus);

        var chance = Math.Clamp(BaseChance + powerDelta + sizeDelta, MinChance, MaxChance);

        var roll = rollProvider.NextRoll();
        var succeeded = roll <= chance;

        var factors = new List<ModelMissionFactor>
        {
            new("Squad strength",
                $"Average power {averagePower:0.#} vs par {par} for a {mission.Difficulty.ToLowerInvariant()} mission",
                powerDelta),
            new("Squad size",
                squad.Count < mission.RequiredHeroCount
                    ? $"{squad.Count} of {mission.RequiredHeroCount} required heroes - understaffed"
                    : squad.Count > mission.RequiredHeroCount
                        ? $"{squad.Count} heroes for a {mission.RequiredHeroCount}-hero mission - reinforced"
                        : $"{squad.Count} heroes, exactly as briefed",
                sizeDelta),
            new("Base odds", "Starting probability before adjustments", BaseChance),
        };

        return new ClsMissionOutcome(succeeded, chance, roll,
            BuildSummary(mission, squad, succeeded, chance, roll), factors);
    }

    private static string BuildSummary(
        ModelMission mission,
        IReadOnlyList<ModelSuperhero> squad,
        bool succeeded,
        int chance,
        int roll)
    {
        var strongest = squad.MaxBy(h => h.PowerLevel)!;
        var squadLabel = squad.Count == 1 ? "operating alone" : $"and {squad.Count - 1} others";

        if (succeeded)
        {
            var margin = chance - roll;
            var how = margin switch
            {
                >= 40 => "The squad was never seriously threatened",
                >= 15 => "The squad completed the objective cleanly",
                _ => "It came down to the wire, but the squad held",
            };

            return $"{how}. {strongest.Name} {squadLabel} secured \"{mission.Title}\" " +
                   $"(rolled {roll} against a {chance}% chance).";
        }

        var shortfall = roll - chance;
        var why = shortfall switch
        {
            >= 40 => "The squad was outmatched from the start",
            >= 15 => "The squad was pushed back and forced to withdraw",
            _ => "The squad came agonisingly close, but fell short",
        };

        return $"{why}. \"{mission.Title}\" was not completed " +
               $"(rolled {roll} against a {chance}% chance).";
    }
}
