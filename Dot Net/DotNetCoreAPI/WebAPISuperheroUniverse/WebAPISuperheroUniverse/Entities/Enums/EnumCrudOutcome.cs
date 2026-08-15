namespace WebAPISuperheroUniverse.Entities.Enums;

/// <summary>
/// Result of a create/update/delete attempt. Returned rather than thrown, because "not found"
/// and "name already taken" are ordinary expected outcomes, not exceptional conditions.
/// </summary>
public enum EnumCrudOutcome
{
    Success,
    NotFound,
    DuplicateName,

    /// <summary>Blocked by existing references - e.g. deleting a hero that already has battle history.</summary>
    InUse
}
