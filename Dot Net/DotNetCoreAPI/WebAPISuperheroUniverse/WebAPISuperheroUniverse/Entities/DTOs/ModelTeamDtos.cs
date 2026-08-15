namespace WebAPISuperheroUniverse.Entities.DTOs;

public sealed record ModelTeamListItem(
    int Id,
    string Name,
    string Universe,
    string? Description,
    DateOnly? FoundedDate,
    int MemberCount);

public sealed record ModelTeamMember(
    int SuperheroId,
    string Name,
    string? RealName,
    string Alignment,
    int PowerLevel,
    string? ImageUrl,
    DateOnly JoinedDate);

/// <summary>Aggregate stats shown on the team detail page.</summary>
public sealed record ModelTeamStatistics(
    int MemberCount,
    int TotalPowerLevel,
    double AveragePowerLevel,
    int StrongestMemberPowerLevel,
    string? StrongestMemberName,
    int HeroCount,
    int VillainCount,
    int AntiHeroCount);

public sealed record ModelTeamDetail(
    int Id,
    string Name,
    string Universe,
    string? Description,
    DateOnly? FoundedDate,
    IReadOnlyList<ModelTeamMember> Members,
    ModelTeamStatistics Statistics);

public sealed record ModelTeamRequest(
    string Name,
    string Universe,
    string? Description,
    DateOnly? FoundedDate);

public sealed record ModelAddTeamMemberRequest(int SuperheroId);
