namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelMissionHero
{
    public int MissionId { get; set; }
    public int SuperheroId { get; set; }

    public ModelMission Mission { get; set; } = null!;
    public ModelSuperhero Superhero { get; set; } = null!;
}
