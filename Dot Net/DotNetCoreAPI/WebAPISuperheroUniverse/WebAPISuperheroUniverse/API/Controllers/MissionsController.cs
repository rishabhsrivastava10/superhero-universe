using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Models;

namespace WebAPISuperheroUniverse.API.Controllers;

[ApiController]
[Route("api/missions")]
[Authorize]
public sealed class MissionsController(IMissionService missionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ModelMissionListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await missionService.GetAllAsync(status, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ModelMissionDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var mission = await missionService.GetByIdAsync(id, cancellationToken);

        return mission is null
            ? NotFound(Error(StatusCodes.Status404NotFound, $"Mission {id} was not found."))
            : Ok(mission);
    }

    /// <summary>Runs a mission attempt. Any signed-in user may do this - it is not an admin action.</summary>
    [HttpPost("{id:int}/start")]
    [ProducesResponseType(typeof(ModelMissionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start(int id, ModelStartMissionRequest request, CancellationToken cancellationToken)
    {
        var (outcome, result) = await missionService.StartAsync(id, request, cancellationToken);

        return outcome switch
        {
            EnumMissionOutcome.Success => Ok(result),
            EnumMissionOutcome.MissionNotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Mission {id} was not found.")),
            EnumMissionOutcome.HeroNotFound => NotFound(Error(StatusCodes.Status404NotFound, "One or more of the selected superheroes was not found.")),
            EnumMissionOutcome.AlreadyResolved => Conflict(Error(StatusCodes.Status409Conflict,
                "This mission has already been completed. Reset it before attempting it again.")),
            EnumMissionOutcome.NoHeroes => BadRequest(Error(StatusCodes.Status400BadRequest, "Select at least one superhero for the mission.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not start the mission.")),
        };
    }

    [HttpPost]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelMissionDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ModelMissionRequest request, CancellationToken cancellationToken)
    {
        var (outcome, mission) = await missionService.CreateAsync(request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => CreatedAtAction(nameof(GetById), new { id = mission!.Id }, mission),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "A mission with that title already exists.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not create mission.")),
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelMissionDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, ModelMissionRequest request, CancellationToken cancellationToken)
    {
        var (outcome, mission) = await missionService.UpdateAsync(id, request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => Ok(mission),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Mission {id} was not found.")),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "Another mission already uses that title.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not update mission.")),
        };
    }

    [HttpPost("{id:int}/reset")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reset(int id, CancellationToken cancellationToken)
    {
        var outcome = await missionService.ResetAsync(id, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => NoContent(),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Mission {id} was not found.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not reset mission.")),
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var outcome = await missionService.DeleteAsync(id, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => NoContent(),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Mission {id} was not found.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not delete mission.")),
        };
    }

    private static ModelErrorResponse Error(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Timestamp = DateTime.UtcNow,
    };
}
