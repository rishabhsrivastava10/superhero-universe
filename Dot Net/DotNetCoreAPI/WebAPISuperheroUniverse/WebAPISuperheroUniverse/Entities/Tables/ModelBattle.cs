namespace WebAPISuperheroUniverse.Entities.Tables;

public class ModelBattle
{
    public int Id { get; set; }
    public int Hero1Id { get; set; }
    public int Hero2Id { get; set; }
    public int? WinnerId { get; set; }
    public int Hero1Score { get; set; }
    public int Hero2Score { get; set; }
    public DateTime BattleDate { get; set; }

    public ModelSuperhero Hero1 { get; set; } = null!;
    public ModelSuperhero Hero2 { get; set; } = null!;
    public ModelSuperhero? Winner { get; set; }
}
