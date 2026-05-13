using System.Security.Claims;
using MediRecords.Domain.Enums;
using MediRecords.Dto.LabOrderDtos; 
using MediRecords.Services.LabOrderServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LabOrderController : ControllerBase
{
    private readonly ILabOrderService _labOrderService;

    public LabOrderController(ILabOrderService labOrderService)
    {
        _labOrderService = labOrderService;
    }

    /// <summary>
    /// Creates a new lab order. Only providers (Physicians) can create lab orders.
    /// </summary>
    /// <param name="requestDto">The lab order request data transfer object containing lab order details.</param>
    /// <returns>Returns the created lab order or an error message.</returns>
    [HttpPost]
    [Authorize(Roles =nameof(UserRoleEnums.Physician))]
    [ProducesResponseType(typeof(LabOrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLabOrder([FromBody] LabOrderCreateRequestDto requestDto)
    {
        if (!ModelState.IsValid || requestDto == null)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "User identification failed." });

            var providerId = int.Parse(userIdClaim.Value);
            var result = await _labOrderService.CreateLabOrderAsync(requestDto, providerId);

            return Created("", result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a specific lab order by its ID.
    /// </summary>
    /// <param name="id">The numeric ID of the lab order.</param>
    /// <returns>Returns the lab order details or a not found error.</returns>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(LabOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLabOrderById(int id)
    {
        try
        {
            var result = await _labOrderService.GetLabOrderByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"Lab order with ID {id} not found." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all lab orders for a specific encounter.
    /// </summary>
    /// <param name="encounterId">The numeric ID of the encounter.</param>
    /// <returns>Returns a list of lab orders for the encounter or a not found error.</returns>
    [HttpGet("encounter/{encounterId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LabOrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLabOrdersByEncounterId(int encounterId)
    {
        try
        {
            var result = await _labOrderService.GetLabOrdersByEncounterIdAsync(encounterId);
            if (result == null || !result.Any())
                return NotFound(new { message = $"No lab orders found for encounter ID {encounterId}." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves lab orders with optional filters.
    /// </summary>
    /// <param name="filters">The filter criteria for lab orders.</param>
    /// <returns>Returns a list of lab orders matching the filters or a not found error.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LabOrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLabOrders([FromQuery] LabOrderFilterRequestDto filters)
    {
        try
        {
            var result = await _labOrderService.GetLabOrdersAsync(filters);
            if (result == null || !result.Any())
                return NotFound(new { message = "No lab orders found matching the provided filters." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError, error = ex.Message });
        }
    }

    /// <summary>
    /// Updates the status of a lab order. Only lab technicians can update lab order status.
    /// </summary>
    /// <param name="id">The ID of the lab order to update.</param>
    /// <param name="status">The new status for the lab order (true for Completed, false for Ordered).</param>
    /// <returns>Returns the updated lab order or an error message.</returns>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = nameof(UserRoleEnums.LabTech))]
    [ProducesResponseType(typeof(LabOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLabOrderStatus(int id, [FromBody] bool status)
    {
        try
        {
            var result = await _labOrderService.UpdateLabOrderStatusAsync(id, status);
            return Ok(result);
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
}
