using System.Security.Claims;
using MediRecords.Dto.ProviderProductivityDtos;
using MediRecords.Services.ProviderProductivityServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ProviderProductivityController : ControllerBase
{
    private readonly IProviderProductivityService _providerProductivityService;

    public ProviderProductivityController(IProviderProductivityService providerProductivityService)
    {
        _providerProductivityService = providerProductivityService;
    }

    /// <summary>
    /// Generates provider productivity metrics including encounters, labs ordered, and prescriptions issued.
    /// </summary>
    /// <param name="providerId">Optional provider ID. If omitted, returns metrics for all providers.</param>
    /// <param name="fromDate">Start date for the report period (required).</param>
    /// <param name="toDate">End date for the report period (required).</param>
    /// <returns>List of provider productivity metrics.</returns>
    [Authorize(Roles = Constant.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProviderProductivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProviderProductivity([FromQuery] int? providerId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        if (fromDate == default || toDate == default)
            return BadRequest(new { message = "fromDate and toDate are required." });

        if (fromDate > toDate)
            return BadRequest(new { message = "fromDate cannot be after toDate." });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Invalid user token." });

        try
        {
            var result = await _providerProductivityService.GetProviderProductivityAsync(providerId, fromDate, toDate, userId);

            if (providerId.HasValue && !result.Any())
                return NotFound(new { message = "Provider not found." });

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = Constant.InternalError });
        }
    }
}