namespace WebAPISuperheroUniverse.Entities.Models;

public sealed class ModelErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
