namespace WebAPISuperheroUniverse.Entities.DTOs;

/// <summary>
/// Standard envelope for every paged list endpoint, so the client can render pagination
/// controls without each endpoint inventing its own shape.
/// </summary>
public sealed record ModelPagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
