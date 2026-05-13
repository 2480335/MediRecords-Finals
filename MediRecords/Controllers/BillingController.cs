using MediRecords.Dto.BillingDtos.Request;
using MediRecords.Dto.BillingDtos.Response;
using MediRecords.Dto.ProcedureCodeDtos;
using MediRecords.Services.BillingServices;
using MediRecords.Services.ProcedureCodeServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Security.Claims;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    private readonly IProcedureCodeService _service;

    public BillingController(IBillingService billingService, IProcedureCodeService service)
    {
        _billingService = billingService;
        _service = service;
    }

    [Authorize(Roles = $"{Constant.Physician},{Constant.Admin}")]
    [HttpPost("visit-charges")]
    [ProducesResponseType(typeof(VisitChargeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignVisitCharge([FromBody] AssignVisitChargeRequestDto dto)
    {
        var (success, message, data, statusCode) =
            await _billingService.AssignVisitChargeAsync(dto);

        return statusCode switch
        {
            201 => CreatedAtAction(nameof(AssignVisitCharge),
                       new { id = data!.ChargeId }, data),
            404 => NotFound(new { message }),
            409 => Conflict(new { message }),
            500 => StatusCode(500, new { message }),
            _ => BadRequest(new { message })
        };
    }

    [Authorize(Roles = Constant.Admin)]
    [HttpPut("visit-charges/{id}")]
    [ProducesResponseType(typeof(VisitChargeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateVisitChargeAmount(
        int id, [FromBody] UpdateVisitChargeRequestDto dto)
    {
        var (success, message, data, statusCode) =
            await _billingService.UpdateVisitChargeAmountAsync(id, dto);

        return statusCode switch
        {
            200 => Ok(data),
            404 => NotFound(new { message }),
            500 => StatusCode(500, new { message }),
            _ => BadRequest(new { message })
        };
    }

    [Authorize(Roles = $"{Constant.Physician},{Constant.Admin}")]
    [HttpGet("encounters/{id}/charges")]
    [ProducesResponseType(typeof(IEnumerable<VisitChargeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChargesByEncounterId(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = Constant.BillingMessages.InvalidEncounterId });

        var (success, message, data, statusCode) =
            await _billingService.GetChargesByEncounterIdAsync(id);

        return statusCode switch
        {
            200 => Ok(data),
            404 => NotFound(new { message }),
            500 => StatusCode(500, new { message }),
            _ => BadRequest(new { message })
        };
    }

    [Authorize(Roles = $"{Constant.Admin},{Constant.FrontDesk}")]
    [HttpGet("unbilled-encounters")]
    [ProducesResponseType(typeof(PagedResponseDto<UnbilledEncounterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUnbilledEncounters(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? providerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (success, message, data, statusCode) =
            await _billingService.GetUnbilledEncountersAsync(
                fromDate, toDate, providerId, page, pageSize);

        return statusCode switch
        {
            200 => Ok(data),
            500 => StatusCode(500, new { message }),
            _ => BadRequest(new { message })
        };
    }

    [Authorize(Roles = Constant.Admin)]
    [HttpPut("visit-charges/mark-billed")]
    [ProducesResponseType(typeof(MarkBilledResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkChargesAsBilled([FromBody] MarkChargesBilledRequestDto dto)
    {
        // Extract UserId from JWT for audit log
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Invalid or missing token." });

        var (success, message, data, statusCode) =
            await _billingService.MarkChargesAsBilledAsync(dto, userId);

        return statusCode switch
        {
            200 => Ok(data),
            500 => StatusCode(500, new { message }),
            _ => BadRequest(new { message })
        };
    }

    [Authorize(Roles = Constant.Admin)]
    [HttpGet("exports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportCharges(
        [FromQuery] string format,
        [FromQuery] string status = "All",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var (success, message, fileContent, contentType, fileName, statusCode) =
            await _billingService.ExportChargesAsync(format, status, fromDate, toDate);

        if (!success)
        {
            return statusCode switch
            {
                500 => StatusCode(500, new { message }),
                _ => BadRequest(new { message })
            };
        }
        return File(fileContent!, contentType, fileName);
    }

    /// <summary>
    /// this is used to pass the data to the service layer
    /// </summary>
    /// <param name="dto">data that has to be inserted</param>
    /// <returns></returns>
    [HttpPost("procedure-codes")]
    [Authorize(Roles = Utility.Constant.Admin)]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProcedureCode(ProcedureCodeRequestDto dto)
    {
        try
        {
            await _service.AddAsync(dto);
            return StatusCode(StatusCodes.Status201Created, Utility.Constant.ProblemCreated);
        }
        catch (BadHttpRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, Utility.Constant.InternalServerError);
        }
    }

    /// <summary>
    /// this route is for getting all the procedure code
    /// </summary>
    /// <returns>this will return all the procedure code</returns>
    [HttpGet("procedure-codes")]
    [Authorize(Roles = $"{Utility.Constant.Physician},{Utility.Constant.Admin}")]
    [ProducesResponseType(typeof(IEnumerable<ProcedureCodeViewDtos>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProcedure([FromQuery] string? filterCode)
    {
        try
        {
            var procedures = await _service.GetAllProcedure(filterCode ?? string.Empty);
            return Ok(procedures);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, Utility.Constant.InternalError);
        }
    }

    /// <summary>
    /// this route is for update the details of procedure code
    /// </summary>
    /// <param name="CodeId">this is for updating the data for this CodeId</param>
    /// <param name="dto">data that has to be updated</param>
    /// <returns></returns>
    [HttpPatch("procedure-codes/{id}")]
    [Authorize(Roles = Utility.Constant.Admin)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcedureCodeUpdate(int id, ProcedureCodeUpdateDtos dto)
    {
        try
        {
            await _service.UpdateProcedureAsync(id, dto);
            return Ok(Utility.Constant.ProcdureUpdated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, Utility.Constant.InternalServerError);
        }
    }
}