using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Class;

/// <summary>
/// Outcome of a simulated battle, before it is persisted.
/// </summary>
public sealed record ClsBattleOutcome(
    int Hero1Score,
    int Hero2Score,
    int? WinnerId,
    bool IsDraw,
    string Summary,
    IReadOnlyList<ModelAttributeComparison> Breakdown);

/// <summary>
/// The battle algorithm. Deliberately free of any database or HTTP dependency: it takes two
/// superheroes and returns an outcome, which makes it trivial to unit test exhaustively.
///
/// This lives on the server, not in Angular, because it IS the business rule - a client-side
/// implementation could be edited by anyone with devtools open.
/// </summary>
public sealed class ClsBattleCalculator(IBattleVarianceProvider varianceProvider)
{
    /// <summary>
    /// Attribute weights, summing to 1.0.
    ///
    /// PowerLevel carries the most weight because it represents a character's overall standing,
    /// while Combat and Strength matter more than raw Intelligence in a physical confrontation.
    /// Intelligence still counts - it models tactics, and is what lets a Batman meaningfully
    /// threaten someone physically stronger.
    /// </summary>
    private static readonly (string Name, double Weight, Func<ModelSuperhero, int> Selector)[] Attributes =
    [
        ("PowerLevel",   0.30, h => h.PowerLevel),
        ("Strength",     0.16, h => h.Strength),
        ("Combat",       0.16, h => h.Combat),
        ("Speed",        0.14, h => h.Speed),
        ("Durability",   0.14, h => h.Durability),
        ("Intelligence", 0.10, h => h.Intelligence),
    ];

    public ClsBattleOutcome Simulate(ModelSuperhero hero1, ModelSuperhero hero2)
    {
        ArgumentNullException.ThrowIfNull(hero1);
        ArgumentNullException.ThrowIfNull(hero2);

        var baseScore1 = WeightedScore(hero1);
        var baseScore2 = WeightedScore(hero2);

        // Each combatant rolls independently, so a weaker hero can occasionally upset a
        // stronger one - but only within a bounded range, so results stay believable.
        var score1 = Clamp(baseScore1 * varianceProvider.NextVariance());
        var score2 = Clamp(baseScore2 * varianceProvider.NextVariance());

        var breakdown = Attributes
            .Select(a =>
            {
                var v1 = a.Selector(hero1);
                var v2 = a.Selector(hero2);
                return new ModelAttributeComparison(
                    a.Name, v1, v2,
                    WonBy: v1 == v2 ? 0 : v1 > v2 ? 1 : 2,
                    WeightPercent: (int)Math.Round(a.Weight * 100));
            })
            .ToList();

        int? winnerId;
        bool isDraw;

        if (score1 > score2)
        {
            winnerId = hero1.Id;
            isDraw = false;
        }
        else if (score2 > score1)
        {
            winnerId = hero2.Id;
            isDraw = false;
        }
        else
        {
            // Scores tied after rounding. Combat skill decides a close fight; if that is also
            // level the battle is a genuine draw, which xtBattles supports via a nullable WinnerId.
            if (hero1.Combat > hero2.Combat)
            {
                winnerId = hero1.Id;
                isDraw = false;
            }
            else if (hero2.Combat > hero1.Combat)
            {
                winnerId = hero2.Id;
                isDraw = false;
            }
            else
            {
                winnerId = null;
                isDraw = true;
            }
        }

        return new ClsBattleOutcome(
            score1, score2, winnerId, isDraw,
            BuildSummary(hero1, hero2, score1, score2, winnerId, isDraw, breakdown),
            breakdown);
    }

    private static double WeightedScore(ModelSuperhero hero) =>
        Attributes.Sum(a => a.Selector(hero) * a.Weight);

    // Scores are presented on the same 0-100 scale as the attributes that produced them.
    private static int Clamp(double value) => (int)Math.Round(Math.Clamp(value, 0, 100));

    private static string BuildSummary(
        ModelSuperhero hero1,
        ModelSuperhero hero2,
        int score1,
        int score2,
        int? winnerId,
        bool isDraw,
        IReadOnlyList<ModelAttributeComparison> breakdown)
    {
        if (isDraw)
        {
            return $"{hero1.Name} and {hero2.Name} fought to a standstill at {score1} apiece - " +
                   "neither could find an opening.";
        }

        var winnerIsHero1 = winnerId == hero1.Id;
        var winner = winnerIsHero1 ? hero1 : hero2;
        var loser = winnerIsHero1 ? hero2 : hero1;
        // Report the winner's score first, otherwise "Thor defeated Batman 78-92" reads backwards.
        var winnerScore = winnerIsHero1 ? score1 : score2;
        var loserScore = winnerIsHero1 ? score2 : score1;
        var margin = Math.Abs(score1 - score2);

        // The attribute the winner led by the most explains WHY they won, which is more
        // interesting than just reporting the score.
        var decisive = breakdown
            .Where(b => b.WonBy == (winnerIsHero1 ? 1 : 2))
            .OrderByDescending(b => Math.Abs(b.Hero1Value - b.Hero2Value) * b.WeightPercent)
            .FirstOrDefault();

        var how = margin switch
        {
            <= 3 => "in a razor-thin contest",
            <= 10 => "after a hard-fought exchange",
            <= 25 => "with a clear advantage",
            _ => "in a decisive rout",
        };

        var because = decisive is null
            ? string.Empty
            : $", carried by superior {decisive.Attribute.ToLowerInvariant()} " +
              $"({(winnerIsHero1 ? decisive.Hero1Value : decisive.Hero2Value)} vs " +
              $"{(winnerIsHero1 ? decisive.Hero2Value : decisive.Hero1Value)})";

        return $"{winner.Name} defeated {loser.Name} {winnerScore}-{loserScore} {how}{because}.";
    }
}
