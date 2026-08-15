namespace WebAPISuperheroUniverse.Entities.Models;

/// <summary>
/// The single error shape returned by every failing endpoint, so the Angular client can handle
/// errors generically instead of per-endpoint.
/// </summary>
public sealed class ModelErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Field-level validation failures, keyed by property name. Omitted from the JSON when there are none.</summary>
    public IDictionary<string, string[]>? Errors { get; init; }
}
