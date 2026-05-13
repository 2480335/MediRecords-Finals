using System.Net;
using System.Security.Claims;
using MediRecords.Dto.VitalSignDtos;
using MediRecords.Services.VitalSignServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class VitalSignController : ControllerBase
{
    private readonly IVitalSignService _vitalSignService;

    public VitalSignController(IVitalSignService vitalSignService)
    {
        _vitalSignService = vitalSignService;
    }

    /// <summary>
    /// Captures vital signs for an encounter, automatically calculates BMI if height and weight are provided.
    /// </summary>
    /// <param name="requestDto">The vital signs data transfer object.</param>
    /// <returns>201 Created with VitalId</returns>
    [HttpPost("capture")]
    [Authorize] 
    [ProducesResponseType(typeof(VitalSignCreateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CaptureVitals(VitalSignCreateRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var response = await _vitalSignService.CreateVitalSignsAsync(requestDto, userId);

            // Updated route values to match the property name in your Response DTO
            return CreatedAtAction(nameof(CaptureVitals), new { id = response.VitalId }, response);
        }
        catch (MediRecordsException ex) when (ex.Message.Contains("closed", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status409Conflict, ex.Message);
        }
        catch (MediRecordsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred: " + ex.Message);
        }
    }
}