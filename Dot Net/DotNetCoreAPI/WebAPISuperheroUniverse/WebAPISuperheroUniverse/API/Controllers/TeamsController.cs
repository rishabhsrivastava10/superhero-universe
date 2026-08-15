using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Models;

namespace WebAPISuperheroUniverse.API.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public sealed class TeamsController(ITeamService teamService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ModelTeamListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? universe, CancellationToken cancellationToken) =>
        Ok(await teamService.GetAllAsync(universe, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ModelTeamDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var team = await teamService.GetByIdAsync(id, cancellationToken);

        return team is null
            ? NotFound(Error(StatusCodes.Status404NotFound, $"Team {id} was not found."))
            : Ok(team);
    }

    [HttpPost]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelTeamDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ModelTeamRequest request, CancellationToken cancellationToken)
    {
        var (outcome, team) = await teamService.CreateAsync(request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => CreatedAtAction(nameof(GetById), new { id = team!.Id }, team),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "A team with that name already exists.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not create team.")),
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelTeamDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, ModelTeamRequest request, CancellationToken cancellationToken)
    {
        var (outcome, team) = await teamService.UpdateAsync(id, request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => Ok(team),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Team {id} was not found.")),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "Another team already uses that name.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not update team.")),
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var outcome = await teamService.DeleteAsync(id, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => NoContent(),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Team {id} was not found.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not delete team.")),
        };
    }

    [HttpPost("{id:int}/members")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMember(int id, ModelAddTeamMemberRequest request, CancellationToken cancellationToken)
    {
        var outcome = await teamService.AddMemberAsync(id, request.SuperheroId, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => NoContent(),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, "The team or superhero was not found.")),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "That superhero is already on this team.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not add team member.")),
        };
    }

    [HttpDelete("{id:int}/members/{superheroId:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(int id, int superheroId, CancellationToken cancellationToken)
    {
        var outcome = await teamService.RemoveMemberAsync(id, superheroId, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => NoContent(),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, "That superhero is not on this team.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not remove team member.")),
        };
    }

    private static ModelErrorResponse Error(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Timestamp = DateTime.UtcNow,
    };
}
