using MediRecords.Dto.LabResultDtos;
using MediRecords.Services.LabResultServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediRecords.Domain.Enums;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LabResultController : ControllerBase
{
    private readonly ILabResultService _labResultService;

    public LabResultController(ILabResultService labResultService)
    {
        _labResultService = labResultService;
    }

    /// <summary>
    /// Creates a new lab result for an existing lab order. Only lab technicians can create lab results.
    /// </summary>
    /// <param name="requestDto">The lab result request data transfer object containing result details.</param>
    /// <returns>Returns the created lab result or an error message.</returns>
    [HttpPost]
    [Authorize(Roles = nameof(UserRoleEnums.LabTech))]
    [ProducesResponseType(typeof(LabResultResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLabResult([FromBody] LabResultRequestDto requestDto)
    {
        if (!ModelState.IsValid || requestDto == null)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _labResultService.CreateLabResultAsync(requestDto);
            return Created("", result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a specific lab result by its ID.
    /// </summary>
    /// <param name="id">The numeric ID of the lab result.</param>
    /// <returns>Returns the lab result details or a not found error.</returns>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(LabResultResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLabResultById(int id)
    {
        try
        {
            var result = await _labResultService.GetLabResultByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"Lab result with ID {id} not found." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all lab results for a specific lab order.
    /// </summary>
    /// <param name="labOrderId">The numeric ID of the lab order.</param>
    /// <returns>Returns a list of lab results for the lab order or a not found error.</returns>
    [HttpGet("laborder/{labOrderId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LabResultResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLabResultsByLabOrderId(int labOrderId)
    {
        try
        {
            var result = await _labResultService.GetLabResultsByLabOrderIdAsync(labOrderId);
            if (result == null || !result.Any())
                return NotFound(new { message = $"No lab results found for lab order ID {labOrderId}." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves lab results with optional filters.
    /// </summary>
    /// <param name="filters">The filter criteria for lab results.</param>
    /// <returns>Returns a list of lab results matching the filters or a not found error.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LabResultResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLabResults([FromQuery] LabResultRequestDto filters)
    {
        try
        {
            var result = await _labResultService.GetLabResultsAsync(filters);
            if (result == null || !result.Any())
                return NotFound(new { message = "No lab results found matching the provided filters." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }
}
