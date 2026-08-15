namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelTeam
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Universe { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? FoundedDate { get; set; }

    public ICollection<ModelSuperheroTeam> SuperheroTeams { get; set; } = [];
}
