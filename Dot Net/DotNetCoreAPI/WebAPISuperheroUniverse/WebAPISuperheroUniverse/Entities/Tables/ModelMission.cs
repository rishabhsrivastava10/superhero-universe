namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelMission
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int RequiredHeroCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<ModelMissionHero> MissionHeroes { get; set; } = [];
}
