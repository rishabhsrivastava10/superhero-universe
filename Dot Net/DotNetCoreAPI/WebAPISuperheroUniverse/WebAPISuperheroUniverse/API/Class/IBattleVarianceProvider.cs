namespace WebAPISuperheroUniverse.API.Class;

/// <summary>
/// Supplies the "luck" factor applied to each combatant's score.
///
/// This exists purely so the randomness is injectable. Calling Random directly inside the
/// calculator would make the battle algorithm untestable - you could never assert an exact
/// outcome. Tests substitute a fixed provider and get fully deterministic results.
/// </summary>
public interface IBattleVarianceProvider
{
    /// <summary>Returns a multiplier around 1.0 (e.g. 0.88 - 1.12) applied to a raw score.</summary>
    double NextVariance();
}

public sealed class ClsBattleVarianceProvider : IBattleVarianceProvider
{
    /// <summary>Maximum swing in either direction. 0.12 = a hero can roll 12% above or below par.</summary>
    public const double MaxVariance = 0.12;

    public double NextVariance() =>
        // Random.Shared is thread-safe, unlike a shared `new Random()` instance.
        1.0 + ((Random.Shared.NextDouble() * 2.0 - 1.0) * MaxVariance);
}

/// <summary>Fixed variance, used by tests to make battles fully deterministic.</summary>
public sealed class ClsFixedVarianceProvider(double variance) : IBattleVarianceProvider
{
    public double NextVariance() => variance;
}
