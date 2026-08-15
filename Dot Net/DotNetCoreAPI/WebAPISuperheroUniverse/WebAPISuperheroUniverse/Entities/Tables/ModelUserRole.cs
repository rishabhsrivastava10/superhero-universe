namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelUserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public ModelUser User { get; set; } = null!;
    public ModelRole Role { get; set; } = null!;
}
