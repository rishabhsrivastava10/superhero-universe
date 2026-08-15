namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelSuperheroPower
{
    public int SuperheroId { get; set; }
    public int PowerId { get; set; }

    public ModelSuperhero Superhero { get; set; } = null!;
    public ModelPower Power { get; set; } = null!;
}
