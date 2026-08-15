namespace WebAPISuperheroUniverse.Entities.DTOs;

public sealed record ModelMissionListItem(
    int Id,
    string Title,
    string? Description,
    string? Location,
    string Difficulty,
    int RequiredHeroCount,
    string Status,
    int AssignedHeroCount,
    DateTime CreatedAt);

public sealed record ModelMissionHeroSummary(
    int SuperheroId,
    string Name,
    string Alignment,
    int PowerLevel,
    string? ImageUrl);

public sealed record ModelMissionDetail(
    int Id,
    string Title,
    string? Description,
    string? Location,
    string Difficulty,
    int RequiredHeroCount,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<ModelMissionHeroSummary> AssignedHeroes);

public sealed record ModelMissionRequest(
    string Title,
    string? Description,
    string? Location,
    string Difficulty,
    int RequiredHeroCount);

/// <summary>Body for POST /api/missions/{id}/start - the squad chosen for the attempt.</summary>
public sealed record ModelStartMissionRequest(IReadOnlyList<int> SuperheroIds);

/// <summary>One factor that fed into the success probability, for an explainable result.</summary>
public sealed record ModelMissionFactor(string Label, string Detail, int ContributionPercent);

public sealed record ModelMissionResult(
    int MissionId,
    string Title,
    string Difficulty,
    /// <summary>"Success" or "Failed".</summary>
    string Status,
    bool Succeeded,
    int SuccessChancePercent,
    int RollPercent,
    string Summary,
    IReadOnlyList<ModelMissionHeroSummary> Squad,
    IReadOnlyList<ModelMissionFactor> Factors);
