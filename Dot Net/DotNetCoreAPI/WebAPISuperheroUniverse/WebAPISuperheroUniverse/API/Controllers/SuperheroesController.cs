using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Models;

namespace WebAPISuperheroUniverse.API.Controllers;

[ApiController]
[Route("api/superheroes")]
// Reading requires a signed-in user; individual mutations additionally require Admin.
[Authorize]
public sealed class SuperheroesController(ISuperheroService superheroService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ModelPagedResponse<ModelSuperheroListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] ModelSuperheroQuery query, CancellationToken cancellationToken) =>
        Ok(await superheroService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ModelSuperheroDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var superhero = await superheroService.GetByIdAsync(id, cancellationToken);

        return superhero is null
            ? NotFound(Error(StatusCodes.Status404NotFound, $"Superhero {id} was not found."))
            : Ok(superhero);
    }

    [HttpPost]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelSuperheroDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ModelSuperheroCreateRequest request, CancellationToken cancellationToken)
    {
        var (outcome, superhero) = await superheroService.CreateAsync(request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => CreatedAtAction(nameof(GetById), new { id = superhero!.Id }, superhero),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "A superhero with that name already exists.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not create superhero."))
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelSuperheroDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, ModelSuperheroUpdateRequest request, CancellationToken cancellationToken)
    {
        var (outcome, superhero) = await superheroService.UpdateAsync(id, request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => Ok(superhero),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Superhero {id} was not found.")),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "Another superhero already uses that name.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not update superhero."))
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var outcome = await superheroService.DeleteAsync(id, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => NoContent(),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Superhero {id} was not found.")),
            EnumCrudOutcome.InUse => Conflict(Error(StatusCodes.Status409Conflict,
                "This superhero has battle history and cannot be deleted. Battle records are kept as a permanent history.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not delete superhero."))
        };
    }

    private static ModelErrorResponse Error(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Timestamp = DateTime.UtcNow
    };
}
