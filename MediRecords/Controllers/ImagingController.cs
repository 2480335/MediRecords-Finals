using MediRecords.Dto.ImagingDtos.Response;
using MediRecords.Services.ImagingServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ImagingController : ControllerBase
{
    private readonly IImagingService _imagingService;

    public ImagingController(IImagingService imagingService)
    {
        _imagingService = imagingService;
    }

    [Authorize(Roles = Constant.Physician)]
    [HttpGet("orders/{orderId}/reports")]
    [ProducesResponseType(typeof(IEnumerable<ImagingReportResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReportsByOrderId(int orderId)
    {
        if (orderId <= 0)
            return BadRequest(new { message = Constant.ImagingMessages.InvalidImagingOrderId });

        var (success, message, data) = await _imagingService.GetReportsByOrderIdAsync(orderId);

        if (!success)
        {
            if (message == Constant.ImagingMessages.ImagingOrderNotFound ||
                message == Constant.ImagingMessages.NoReportsFound)
                return NotFound(new { message });

            return BadRequest(new { message });
        }

        return Ok(data);
    }
}