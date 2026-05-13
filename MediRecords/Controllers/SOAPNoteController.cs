using MediRecords.Dto.SOAPNoteDtos.Request;
using MediRecords.Dto.SOAPNoteDtos.Response;
using MediRecords.Services.SOAPNoteService;
using Microsoft.AspNetCore.Authorization;
using MediRecords.Utility;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class SOAPNoteController : ControllerBase
{
    private readonly ISOAPNoteService _soapNoteService;

    public SOAPNoteController(ISOAPNoteService soapNoteService)
    {
        _soapNoteService = soapNoteService;
    }

    [Authorize(Roles = Constant.Physician)]
    [HttpPost("soap")]
    [ProducesResponseType(typeof(SOAPNoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveSOAPNote([FromBody] SaveSOAPNoteRequestDto dto)
    {
        if (dto.EncounterId <= 0)
            return BadRequest(new { message = Constant.SOAPNoteMessages.InvalidEncounterId });

        var (success, message, data) = await _soapNoteService.SaveSOAPNoteAsync(dto.EncounterId, dto);

        if (!success)
        {
            if (message == Constant.SOAPNoteMessages.EncounterNotFound)
                return NotFound(new { message });

            return BadRequest(new { message });
        }

        return Ok(data);
    }
}