namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelRole
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ModelUserRole> UserRoles { get; set; } = [];
}
