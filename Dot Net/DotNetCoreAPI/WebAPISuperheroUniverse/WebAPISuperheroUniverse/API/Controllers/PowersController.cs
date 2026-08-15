using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Models;

namespace WebAPISuperheroUniverse.API.Controllers;

[ApiController]
[Route("api/powers")]
[Authorize]
public sealed class PowersController(IPowerService powerService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ModelPowerListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await powerService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ModelPowerListItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var power = await powerService.GetByIdAsync(id, cancellationToken);

        return power is null
            ? NotFound(Error(StatusCodes.Status404NotFound, $"Power {id} was not found."))
            : Ok(power);
    }

    [HttpPost]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelPowerListItem), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ModelPowerRequest request, CancellationToken cancellationToken)
    {
        var (outcome, power) = await powerService.CreateAsync(request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => CreatedAtAction(nameof(GetById), new { id = power!.Id }, power),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "A power with that name already exists.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not create power.")),
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(typeof(ModelPowerListItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, ModelPowerRequest request, CancellationToken cancellationToken)
    {
        var (outcome, power) = await powerService.UpdateAsync(id, request, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => Ok(power),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Power {id} was not found.")),
            EnumCrudOutcome.DuplicateName => Conflict(Error(StatusCodes.Status409Conflict, "Another power already uses that name.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not update power.")),
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var outcome = await powerService.DeleteAsync(id, cancellationToken);

        return outcome switch
        {
            EnumCrudOutcome.Success => NoContent(),
            EnumCrudOutcome.NotFound => NotFound(Error(StatusCodes.Status404NotFound, $"Power {id} was not found.")),
            EnumCrudOutcome.InUse => Conflict(Error(StatusCodes.Status409Conflict,
                "This power is assigned to one or more superheroes. Unassign it before deleting.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Could not delete power.")),
        };
    }

    private static ModelErrorResponse Error(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Timestamp = DateTime.UtcNow,
    };
}
