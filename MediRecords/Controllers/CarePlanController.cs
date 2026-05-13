using MediRecords.Dto.CarePlanDtos.Request;
using MediRecords.Dto.CarePlanDtos.Response;
using MediRecords.Services.CarePlanServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class CarePlanController : ControllerBase
{
    private readonly ICarePlanService _carePlanService;

    public CarePlanController(ICarePlanService carePlanService)
    {
        _carePlanService = carePlanService;
    }
    /// Status: false = Active | true = Completed
    [Authorize(Roles = Constant.Physician)]
    [HttpPost]
    [ProducesResponseType(typeof(CarePlanResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCarePlan([FromBody] CreateCarePlanRequestDto dto)
    {
        var (success, message, data) = await _carePlanService.CreateCarePlanAsync(dto);

        if (!success)
        {
            if (message == Constant.CarePlanMessages.PatientNotFound)
                return NotFound(new { message });

            return BadRequest(new { message }); 
        }
        
        return CreatedAtAction(nameof(CreateCarePlan),
            new { id = data!.CarePlanId }, data);
    }

    [HttpGet]
    [Authorize(Roles = Constant.Physician + "," + Constant.Nurse)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCarePlans(
        [FromQuery] int? patientId,
        [FromQuery] string? patientName,
        [FromQuery] bool? status)
    {
        try
        {
            var result = await _carePlanService.GetCarePlansAsync(patientId, patientName, status);
            if(result == null || !result.Any())
            {
                return NotFound(new
                {
                    message = "No care plan records found matching the provided filters"
                });
            }
            return Ok(result);
        }
        catch (MediRecordsException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Constant.Physician + "," + Constant.Nurse)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCarePlanById(int id)
    {
        try
        {
            var result = await _carePlanService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (MediRecordsException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(ex.Message);

            return BadRequest(ex.Message); 
        }
    }
}