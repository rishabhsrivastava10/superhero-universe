namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelSuperheroTeam
{
    public int SuperheroId { get; set; }
    public int TeamId { get; set; }
    public DateOnly JoinedDate { get; set; }

    public ModelSuperhero Superhero { get; set; } = null!;
    public ModelTeam Team { get; set; } = null!;
}
