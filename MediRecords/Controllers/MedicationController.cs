using MediRecords.Dto.MedicationListDtos;
using MediRecords.Services.MedicationServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class MedicationController : ControllerBase
{
    private readonly IMedicationService _medicationService;

    public MedicationController(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    /// <summary>
    /// Returns medication list records. All filter values are optional.
    /// </summary>
    [HttpGet]
    //[Authorize]
    [ProducesResponseType(typeof(IEnumerable<MedicationListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMedicationList([FromQuery] MedicationListRequestDto filters)
    {
        try
        {
            var result = await _medicationService.GetMedicationListsAsync(filters);
            if(result == null || !result.Any())
            {
                return NotFound(new { message = "No records found matching the provided filters." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = Constant.InternalError + ex.Message });
        }
    }
}
