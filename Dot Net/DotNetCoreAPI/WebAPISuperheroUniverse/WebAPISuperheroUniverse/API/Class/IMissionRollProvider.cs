namespace WebAPISuperheroUniverse.API.Class;

/// <summary>
/// Supplies the 1-100 roll a mission attempt is resolved against.
///
/// Injected for the same reason as the battle variance: calling Random inline would make the
/// mission algorithm impossible to assert on. Tests substitute a fixed roll and get a
/// deterministic outcome.
/// </summary>
public interface IMissionRollProvider
{
    /// <summary>Returns a value in 1..100 inclusive. Success when roll &lt;= success chance.</summary>
    int NextRoll();
}

public sealed class ClsMissionRollProvider : IMissionRollProvider
{
    // Upper bound is exclusive, so 101 yields 1..100.
    public int NextRoll() => Random.Shared.Next(1, 101);
}

/// <summary>Fixed roll, used by tests to force a known outcome.</summary>
public sealed class ClsFixedRollProvider(int roll) : IMissionRollProvider
{
    public int NextRoll() => roll;
}
