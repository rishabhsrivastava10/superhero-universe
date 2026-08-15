namespace WebAPISuperheroUniverse.Entities.DTOs;

public sealed record ModelPowerListItem(int Id, string Name, string? Description, int SuperheroCount);

public sealed record ModelPowerRequest(string Name, string? Description);

/// <summary>Body for PUT /api/superheroes/{id}/powers - replaces the hero's whole power set.</summary>
public sealed record ModelAssignPowersRequest(IReadOnlyList<int> PowerIds);
