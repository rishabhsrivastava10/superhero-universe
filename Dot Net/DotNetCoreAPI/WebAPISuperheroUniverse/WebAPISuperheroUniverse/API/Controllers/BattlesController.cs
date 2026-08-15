using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Models;

namespace WebAPISuperheroUniverse.API.Controllers;

[ApiController]
[Route("api/battles")]
// Any signed-in user may simulate battles and read history - this is not an admin feature.
[Authorize]
public sealed class BattlesController(IBattleService battleService) : ControllerBase
{
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(ModelBattleResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Simulate(ModelSimulateBattleRequest request, CancellationToken cancellationToken)
    {
        var (outcome, result) = await battleService.SimulateAsync(request, cancellationToken);

        return outcome switch
        {
            // 201: a battle record was created, and Location points at it.
            EnumBattleOutcome.Success => CreatedAtAction(nameof(GetById), new { id = result!.BattleId }, result),
            EnumBattleOutcome.SameHero => BadRequest(Error(StatusCodes.Status400BadRequest,
                "A superhero cannot battle themselves. Choose two different heroes.")),
            EnumBattleOutcome.HeroNotFound => NotFound(Error(StatusCodes.Status404NotFound,
                "One or both of the selected superheroes was not found.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not simulate the battle.")),
        };
    }

    [HttpGet]
    [ProducesResponseType(typeof(ModelPagedResponse<ModelBattleListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? superheroId = null,
        CancellationToken cancellationToken = default) =>
        Ok(await battleService.GetHistoryAsync(page, pageSize, superheroId, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ModelBattleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var battle = await battleService.GetByIdAsync(id, cancellationToken);

        return battle is null
            ? NotFound(Error(StatusCodes.Status404NotFound, $"Battle {id} was not found."))
            : Ok(battle);
    }

    private static ModelErrorResponse Error(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Timestamp = DateTime.UtcNow,
    };
}
