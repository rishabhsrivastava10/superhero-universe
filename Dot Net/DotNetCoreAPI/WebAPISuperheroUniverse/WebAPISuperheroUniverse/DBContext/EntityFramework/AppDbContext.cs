using Microsoft.EntityFrameworkCore;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.DBContext.EntityFramework;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ModelUser> Users => Set<ModelUser>();
    public DbSet<ModelRole> Roles => Set<ModelRole>();
    public DbSet<ModelUserRole> UserRoles => Set<ModelUserRole>();
    public DbSet<ModelSuperhero> Superheroes => Set<ModelSuperhero>();
    public DbSet<ModelPower> Powers => Set<ModelPower>();
    public DbSet<ModelSuperheroPower> SuperheroPowers => Set<ModelSuperheroPower>();
    public DbSet<ModelTeam> Teams => Set<ModelTeam>();
    public DbSet<ModelSuperheroTeam> SuperheroTeams => Set<ModelSuperheroTeam>();
    public DbSet<ModelMission> Missions => Set<ModelMission>();
    public DbSet<ModelMissionHero> MissionHeroes => Set<ModelMissionHero>();
    public DbSet<ModelBattle> Battles => Set<ModelBattle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Every mapping below mirrors SQL\DB\Tables\SuperheroUniverse\*.sql exactly (same table/constraint/index
        // names) - that folder is the authoritative schema design; this is the ORM's view onto it.

        modelBuilder.Entity<ModelUser>(entity =>
        {
            entity.ToTable("xtUsers");
            entity.HasKey(e => e.Id).HasName("PK_xtUsers");
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedAt).HasPrecision(3);
            entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("UQ_xtUsers_Username");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_xtUsers_Email");
        });

        modelBuilder.Entity<ModelRole>(entity =>
        {
            entity.ToTable("xtRoles");
            entity.HasKey(e => e.Id).HasName("PK_xtRoles");
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_xtRoles_Name");
        });

        modelBuilder.Entity<ModelUserRole>(entity =>
        {
            entity.ToTable("xtUserRoles");
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK_xtUserRoles");
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId).HasConstraintName("FK_xtUserRoles_xtUsers").OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId).HasConstraintName("FK_xtUserRoles_xtRoles").OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.RoleId).HasDatabaseName("IX_xtUserRoles_RoleId");
        });

        modelBuilder.Entity<ModelSuperhero>(entity =>
        {
            entity.ToTable("xtSuperheroes", t =>
            {
                t.HasCheckConstraint("CK_xtSuperheroes_Alignment", "[Alignment] IN (N'Hero', N'Villain', N'Anti-Hero')");
                t.HasCheckConstraint("CK_xtSuperheroes_PowerLevel", "[PowerLevel] BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_xtSuperheroes_Intelligence", "[Intelligence] BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_xtSuperheroes_Strength", "[Strength] BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_xtSuperheroes_Speed", "[Speed] BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_xtSuperheroes_Durability", "[Durability] BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_xtSuperheroes_Combat", "[Combat] BETWEEN 0 AND 100");
            });
            entity.HasKey(e => e.Id).HasName("PK_xtSuperheroes");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RealName).HasMaxLength(150);
            entity.Property(e => e.Universe).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Alignment).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasPrecision(3);
            entity.Property(e => e.UpdatedAt).HasPrecision(3);
            entity.HasIndex(e => e.Name).HasDatabaseName("IX_xtSuperheroes_Name");
            entity.HasIndex(e => new { e.Universe, e.Alignment }).HasDatabaseName("IX_xtSuperheroes_Universe_Alignment")
                .IncludeProperties(e => e.PowerLevel);
            entity.HasIndex(e => e.PowerLevel).HasDatabaseName("IX_xtSuperheroes_PowerLevel").IsDescending();
        });

        modelBuilder.Entity<ModelPower>(entity =>
        {
            entity.ToTable("xtPowers");
            entity.HasKey(e => e.Id).HasName("PK_xtPowers");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_xtPowers_Name");
        });

        modelBuilder.Entity<ModelSuperheroPower>(entity =>
        {
            entity.ToTable("xtSuperheroPowers");
            entity.HasKey(e => new { e.SuperheroId, e.PowerId }).HasName("PK_xtSuperheroPowers");
            entity.HasOne(e => e.Superhero).WithMany(s => s.SuperheroPowers)
                .HasForeignKey(e => e.SuperheroId).HasConstraintName("FK_xtSuperheroPowers_xtSuperheroes").OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Power).WithMany(p => p.SuperheroPowers)
                .HasForeignKey(e => e.PowerId).HasConstraintName("FK_xtSuperheroPowers_xtPowers").OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.PowerId).HasDatabaseName("IX_xtSuperheroPowers_PowerId");
        });

        modelBuilder.Entity<ModelTeam>(entity =>
        {
            entity.ToTable("xtTeams");
            entity.HasKey(e => e.Id).HasName("PK_xtTeams");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Universe).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_xtTeams_Name");
            entity.HasIndex(e => e.Universe).HasDatabaseName("IX_xtTeams_Universe");
        });

        modelBuilder.Entity<ModelSuperheroTeam>(entity =>
        {
            entity.ToTable("xtSuperheroTeams");
            entity.HasKey(e => new { e.SuperheroId, e.TeamId }).HasName("PK_xtSuperheroTeams");
            entity.HasOne(e => e.Superhero).WithMany(s => s.SuperheroTeams)
                .HasForeignKey(e => e.SuperheroId).HasConstraintName("FK_xtSuperheroTeams_xtSuperheroes").OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Team).WithMany(t => t.SuperheroTeams)
                .HasForeignKey(e => e.TeamId).HasConstraintName("FK_xtSuperheroTeams_xtTeams").OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TeamId).HasDatabaseName("IX_xtSuperheroTeams_TeamId");
        });

        modelBuilder.Entity<ModelMission>(entity =>
        {
            entity.ToTable("xtMissions", t =>
            {
                t.HasCheckConstraint("CK_xtMissions_Difficulty", "[Difficulty] IN (N'Easy', N'Medium', N'Hard')");
                t.HasCheckConstraint("CK_xtMissions_Status", "[Status] IN (N'Pending', N'InProgress', N'Success', N'Failed')");
                t.HasCheckConstraint("CK_xtMissions_RequiredHeroCount", "[RequiredHeroCount] > 0");
            });
            entity.HasKey(e => e.Id).HasName("PK_xtMissions");
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(150);
            entity.Property(e => e.Difficulty).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).HasPrecision(3);
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_xtMissions_Status");
        });

        modelBuilder.Entity<ModelMissionHero>(entity =>
        {
            entity.ToTable("xtMissionHeroes");
            entity.HasKey(e => new { e.MissionId, e.SuperheroId }).HasName("PK_xtMissionHeroes");
            entity.HasOne(e => e.Mission).WithMany(m => m.MissionHeroes)
                .HasForeignKey(e => e.MissionId).HasConstraintName("FK_xtMissionHeroes_xtMissions").OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Superhero).WithMany(s => s.MissionHeroes)
                .HasForeignKey(e => e.SuperheroId).HasConstraintName("FK_xtMissionHeroes_xtSuperheroes").OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.SuperheroId).HasDatabaseName("IX_xtMissionHeroes_SuperheroId");
        });

        modelBuilder.Entity<ModelBattle>(entity =>
        {
            entity.ToTable("xtBattles", t =>
                t.HasCheckConstraint("CK_xtBattles_DistinctHeroes", "[Hero1Id] <> [Hero2Id]"));
            entity.HasKey(e => e.Id).HasName("PK_xtBattles");
            entity.Property(e => e.BattleDate).HasPrecision(3);

            // Three FKs converge on xtSuperheroes - SQL Server forbids cascading deletes here
            // (multiple cascade paths), so all three must be Restrict, matching the raw SQL's NO ACTION.
            entity.HasOne(e => e.Hero1).WithMany()
                .HasForeignKey(e => e.Hero1Id).HasConstraintName("FK_xtBattles_Hero1").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Hero2).WithMany()
                .HasForeignKey(e => e.Hero2Id).HasConstraintName("FK_xtBattles_Hero2").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Winner).WithMany()
                .HasForeignKey(e => e.WinnerId).HasConstraintName("FK_xtBattles_Winner").OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BattleDate).HasDatabaseName("IX_xtBattles_BattleDate").IsDescending();
            entity.HasIndex(e => e.Hero1Id).HasDatabaseName("IX_xtBattles_Hero1Id");
            entity.HasIndex(e => e.Hero2Id).HasDatabaseName("IX_xtBattles_Hero2Id");
        });
    }
}
