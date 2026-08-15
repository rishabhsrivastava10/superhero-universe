namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelSuperhero
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string Universe { get; set; } = string.Empty;
    public string Alignment { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PowerLevel { get; set; }
    public int Intelligence { get; set; }
    public int Strength { get; set; }
    public int Speed { get; set; }
    public int Durability { get; set; }
    public int Combat { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ModelSuperheroPower> SuperheroPowers { get; set; } = [];
    public ICollection<ModelSuperheroTeam> SuperheroTeams { get; set; } = [];
    public ICollection<ModelMissionHero> MissionHeroes { get; set; } = [];
}
