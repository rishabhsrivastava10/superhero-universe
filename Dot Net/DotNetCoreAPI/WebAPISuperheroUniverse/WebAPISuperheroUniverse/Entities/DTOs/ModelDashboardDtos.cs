namespace WebAPISuperheroUniverse.Entities.DTOs;

/// <summary>Headline counts across the whole universe.</summary>
public sealed record ModelDashboardTotals(
    int Superheroes,
    int MarvelHeroes,
    int DcHeroes,
    int OtherUniverseHeroes,
    int Powers,
    int Teams,
    int Battles,
    int Missions,
    int MissionsCompleted,
    int MissionsSucceeded);

/// <summary>A single labelled slice, used for the universe/alignment charts.</summary>
public sealed record ModelChartSlice(string Label, int Value);

public sealed record ModelDashboardHero(
    int Id,
    string Name,
    string Universe,
    string? ImageUrl,
    int PowerLevel,
    int BattlesWon,
    int TotalBattles);

public sealed record ModelDashboardTeam(
    int Id,
    string Name,
    string Universe,
    int MemberCount,
    int TotalPowerLevel);

public sealed record ModelDashboardBattle(
    int Id,
    string Hero1Name,
    string? Hero1ImageUrl,
    int Hero1Score,
    string Hero2Name,
    string? Hero2ImageUrl,
    int Hero2Score,
    string? WinnerName,
    DateTime BattleDate);

public sealed record ModelDashboard(
    ModelDashboardTotals Totals,
    IReadOnlyList<ModelChartSlice> HeroesByUniverse,
    IReadOnlyList<ModelChartSlice> HeroesByAlignment,
    IReadOnlyList<ModelChartSlice> MissionsByStatus,
    IReadOnlyList<ModelDashboardHero> TopSuperheroes,
    IReadOnlyList<ModelDashboardHero> MostVictorious,
    IReadOnlyList<ModelDashboardTeam> LargestTeams,
    IReadOnlyList<ModelDashboardBattle> RecentBattles);

/// <summary>One row of a ranking table.</summary>
public sealed record ModelRankingEntry(
    int Rank,
    int Id,
    string Name,
    string? RealName,
    string Universe,
    string Alignment,
    string? ImageUrl,
    /// <summary>Value of whichever attribute the ranking is sorted by.</summary>
    int Value,
    int BattlesWon,
    int TotalBattles);

public sealed record ModelRankings(
    string SortedBy,
    IReadOnlyList<ModelRankingEntry> Overall,
    IReadOnlyList<ModelRankingEntry> Marvel,
    IReadOnlyList<ModelRankingEntry> Dc);
