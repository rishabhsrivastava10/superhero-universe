namespace WebAPISuperheroUniverse.Entities.DTOs;

public sealed record ModelSimulateBattleRequest(int Hero1Id, int Hero2Id);

/// <summary>One attribute compared across both combatants, for the result breakdown.</summary>
public sealed record ModelAttributeComparison(
    string Attribute,
    int Hero1Value,
    int Hero2Value,
    /// <summary>1 if hero 1 wins this attribute, 2 if hero 2 does, 0 for a tie.</summary>
    int WonBy,
    /// <summary>How much this attribute contributes to the final score, as a percentage.</summary>
    int WeightPercent);

public sealed record ModelBattleCombatant(
    int Id,
    string Name,
    string Universe,
    string Alignment,
    string? ImageUrl,
    int PowerLevel,
    int Score);

public sealed record ModelBattleResult(
    int BattleId,
    ModelBattleCombatant Hero1,
    ModelBattleCombatant Hero2,
    int? WinnerId,
    string? WinnerName,
    int? LoserId,
    string? LoserName,
    bool IsDraw,
    string Summary,
    IReadOnlyList<ModelAttributeComparison> Breakdown,
    DateTime BattleDate);

public sealed record ModelBattleListItem(
    int Id,
    int Hero1Id,
    string Hero1Name,
    string? Hero1ImageUrl,
    int Hero1Score,
    int Hero2Id,
    string Hero2Name,
    string? Hero2ImageUrl,
    int Hero2Score,
    int? WinnerId,
    string? WinnerName,
    DateTime BattleDate);
