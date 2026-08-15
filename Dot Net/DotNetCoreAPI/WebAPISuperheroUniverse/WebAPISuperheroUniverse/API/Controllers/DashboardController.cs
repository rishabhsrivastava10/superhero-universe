using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.Entities.DTOs;

namespace WebAPISuperheroUniverse.API.Controllers;

[ApiController]
[Authorize]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("api/dashboard")]
    [ProducesResponseType(typeof(ModelDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await dashboardService.GetDashboardAsync(cancellationToken));

    /// <param name="sortBy">
    /// powerLevel (default), strength, speed, intelligence, combat, durability or wins.
    /// An unrecognised value falls back to powerLevel rather than erroring.
    /// </param>
    [HttpGet("api/rankings")]
    [ProducesResponseType(typeof(ModelRankings), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRankings(
        [FromQuery] string? sortBy = null,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await dashboardService.GetRankingsAsync(sortBy, take, cancellationToken));
}
