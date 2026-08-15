namespace WebAPISuperheroUniverse.Entities.DTOs;

/// <summary>
/// Query-string filters for GET /api/superheroes. Bound with [FromQuery], so every property
/// is optional and the endpoint works with no parameters at all.
/// </summary>
public sealed class ModelSuperheroQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    /// <summary>Case-insensitive partial match on Name or RealName.</summary>
    public string? Search { get; set; }

    public string? Universe { get; set; }
    public string? Alignment { get; set; }
    public int? MinPowerLevel { get; set; }
    public int? MaxPowerLevel { get; set; }

    /// <summary>One of the keys in <see cref="AllowedSortFields"/>. Anything else is rejected.</summary>
    public string? SortBy { get; set; }

    /// <summary>"asc" or "desc". Defaults to ascending.</summary>
    public string? SortDir { get; set; }

    /// <summary>
    /// Allow-list of sortable fields. Sorting is driven by user input, so it must never be
    /// interpolated into a query - only these known keys map to a column.
    /// </summary>
    public static readonly string[] AllowedSortFields =
    [
        "name", "powerlevel", "intelligence", "strength", "speed", "durability", "combat", "createdat"
    ];
}

/// <summary>Trimmed-down shape for list/grid views - deliberately excludes Description and the stat breakdown.</summary>
public sealed record ModelSuperheroListItem(
    int Id,
    string Name,
    string? RealName,
    string Universe,
    string Alignment,
    int PowerLevel,
    string? ImageUrl,
    IReadOnlyList<string> Powers);

public sealed record ModelSuperheroDetail(
    int Id,
    string Name,
    string? RealName,
    string Universe,
    string Alignment,
    string? Description,
    int PowerLevel,
    int Intelligence,
    int Strength,
    int Speed,
    int Durability,
    int Combat,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<string> Powers,
    IReadOnlyList<string> Teams,
    int TotalBattles,
    int BattlesWon);

public sealed record ModelSuperheroCreateRequest(
    string Name,
    string? RealName,
    string Universe,
    string Alignment,
    string? Description,
    int PowerLevel,
    int Intelligence,
    int Strength,
    int Speed,
    int Durability,
    int Combat,
    string? ImageUrl);

public sealed record ModelSuperheroUpdateRequest(
    string Name,
    string? RealName,
    string Universe,
    string Alignment,
    string? Description,
    int PowerLevel,
    int Intelligence,
    int Strength,
    int Speed,
    int Durability,
    int Combat,
    string? ImageUrl);
