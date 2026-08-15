using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPISuperheroUniverse.DBContext.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "xtMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Difficulty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequiredHeroCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtMissions", x => x.Id);
                    table.CheckConstraint("CK_xtMissions_Difficulty", "[Difficulty] IN (N'Easy', N'Medium', N'Hard')");
                    table.CheckConstraint("CK_xtMissions_RequiredHeroCount", "[RequiredHeroCount] > 0");
                    table.CheckConstraint("CK_xtMissions_Status", "[Status] IN (N'Pending', N'InProgress', N'Success', N'Failed')");
                });

            migrationBuilder.CreateTable(
                name: "xtPowers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtPowers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "xtRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "xtSuperheroes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RealName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Universe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Alignment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    Intelligence = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    Speed = table.Column<int>(type: "int", nullable: false),
                    Durability = table.Column<int>(type: "int", nullable: false),
                    Combat = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtSuperheroes", x => x.Id);
                    table.CheckConstraint("CK_xtSuperheroes_Alignment", "[Alignment] IN (N'Hero', N'Villain', N'Anti-Hero')");
                    table.CheckConstraint("CK_xtSuperheroes_Combat", "[Combat] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_xtSuperheroes_Durability", "[Durability] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_xtSuperheroes_Intelligence", "[Intelligence] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_xtSuperheroes_PowerLevel", "[PowerLevel] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_xtSuperheroes_Speed", "[Speed] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_xtSuperheroes_Strength", "[Strength] BETWEEN 0 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "xtTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Universe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FoundedDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "xtUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "xtBattles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hero1Id = table.Column<int>(type: "int", nullable: false),
                    Hero2Id = table.Column<int>(type: "int", nullable: false),
                    WinnerId = table.Column<int>(type: "int", nullable: true),
                    Hero1Score = table.Column<int>(type: "int", nullable: false),
                    Hero2Score = table.Column<int>(type: "int", nullable: false),
                    BattleDate = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtBattles", x => x.Id);
                    table.CheckConstraint("CK_xtBattles_DistinctHeroes", "[Hero1Id] <> [Hero2Id]");
                    table.ForeignKey(
                        name: "FK_xtBattles_Hero1",
                        column: x => x.Hero1Id,
                        principalTable: "xtSuperheroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_xtBattles_Hero2",
                        column: x => x.Hero2Id,
                        principalTable: "xtSuperheroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_xtBattles_Winner",
                        column: x => x.WinnerId,
                        principalTable: "xtSuperheroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "xtMissionHeroes",
                columns: table => new
                {
                    MissionId = table.Column<int>(type: "int", nullable: false),
                    SuperheroId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtMissionHeroes", x => new { x.MissionId, x.SuperheroId });
                    table.ForeignKey(
                        name: "FK_xtMissionHeroes_xtMissions",
                        column: x => x.MissionId,
                        principalTable: "xtMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_xtMissionHeroes_xtSuperheroes",
                        column: x => x.SuperheroId,
                        principalTable: "xtSuperheroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "xtSuperheroPowers",
                columns: table => new
                {
                    SuperheroId = table.Column<int>(type: "int", nullable: false),
                    PowerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtSuperheroPowers", x => new { x.SuperheroId, x.PowerId });
                    table.ForeignKey(
                        name: "FK_xtSuperheroPowers_xtPowers",
                        column: x => x.PowerId,
                        principalTable: "xtPowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_xtSuperheroPowers_xtSuperheroes",
                        column: x => x.SuperheroId,
                        principalTable: "xtSuperheroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "xtSuperheroTeams",
                columns: table => new
                {
                    SuperheroId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    JoinedDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtSuperheroTeams", x => new { x.SuperheroId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_xtSuperheroTeams_xtSuperheroes",
                        column: x => x.SuperheroId,
                        principalTable: "xtSuperheroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_xtSuperheroTeams_xtTeams",
                        column: x => x.TeamId,
                        principalTable: "xtTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "xtUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xtUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_xtUserRoles_xtRoles",
                        column: x => x.RoleId,
                        principalTable: "xtRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_xtUserRoles_xtUsers",
                        column: x => x.UserId,
                        principalTable: "xtUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_xtBattles_BattleDate",
                table: "xtBattles",
                column: "BattleDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_xtBattles_Hero1Id",
                table: "xtBattles",
                column: "Hero1Id");

            migrationBuilder.CreateIndex(
                name: "IX_xtBattles_Hero2Id",
                table: "xtBattles",
                column: "Hero2Id");

            migrationBuilder.CreateIndex(
                name: "IX_xtBattles_WinnerId",
                table: "xtBattles",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_xtMissionHeroes_SuperheroId",
                table: "xtMissionHeroes",
                column: "SuperheroId");

            migrationBuilder.CreateIndex(
                name: "IX_xtMissions_Status",
                table: "xtMissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UQ_xtPowers_Name",
                table: "xtPowers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_xtRoles_Name",
                table: "xtRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_xtSuperheroes_Name",
                table: "xtSuperheroes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_xtSuperheroes_PowerLevel",
                table: "xtSuperheroes",
                column: "PowerLevel",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_xtSuperheroes_Universe_Alignment",
                table: "xtSuperheroes",
                columns: new[] { "Universe", "Alignment" })
                .Annotation("SqlServer:Include", new[] { "PowerLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_xtSuperheroPowers_PowerId",
                table: "xtSuperheroPowers",
                column: "PowerId");

            migrationBuilder.CreateIndex(
                name: "IX_xtSuperheroTeams_TeamId",
                table: "xtSuperheroTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_xtTeams_Universe",
                table: "xtTeams",
                column: "Universe");

            migrationBuilder.CreateIndex(
                name: "UQ_xtTeams_Name",
                table: "xtTeams",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_xtUserRoles_RoleId",
                table: "xtUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ_xtUsers_Email",
                table: "xtUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_xtUsers_Username",
                table: "xtUsers",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "xtBattles");

            migrationBuilder.DropTable(
                name: "xtMissionHeroes");

            migrationBuilder.DropTable(
                name: "xtSuperheroPowers");

            migrationBuilder.DropTable(
                name: "xtSuperheroTeams");

            migrationBuilder.DropTable(
                name: "xtUserRoles");

            migrationBuilder.DropTable(
                name: "xtMissions");

            migrationBuilder.DropTable(
                name: "xtPowers");

            migrationBuilder.DropTable(
                name: "xtSuperheroes");

            migrationBuilder.DropTable(
                name: "xtTeams");

            migrationBuilder.DropTable(
                name: "xtRoles");

            migrationBuilder.DropTable(
                name: "xtUsers");
        }
    }
}
