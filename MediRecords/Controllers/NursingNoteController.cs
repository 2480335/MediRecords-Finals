using MediRecords.Dto.NursingNoteDtos;
using MediRecords.Services.NursingNoteServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
 
namespace MediRecords.Controllers;
 
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class NursingNoteController : ControllerBase
{
    private readonly INursingNoteService _nursingNoteService;
 
    public NursingNoteController(INursingNoteService nursingNoteService)
    {
        _nursingNoteService = nursingNoteService;
    }
 
    [HttpPost("encounters/{encounterId}/nursing-notes")]
    [ProducesResponseType(typeof(NursingNoteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddNote(int encounterId, NursingNoteCreateRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
 
        var recordedBy = User.FindFirst(ClaimTypes.Name)!.Value;
 
        try
        {
            var response = await _nursingNoteService.AddNoteAsync(encounterId, requestDto, recordedBy);
            return CreatedAtAction(nameof(AddNote), new { id = response.NursingNoteId }, response);
        }
        catch (MediRecordsException ex) when (ex.Message.Contains("closed"))
        {
            return StatusCode(409, ex.Message);
        }
        catch (MediRecordsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, Constant.InternalError);
        }
    }
 
    [HttpPut("{noteId}")]
    [ProducesResponseType(typeof(NursingNoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateNote(int noteId, NursingNoteUpdateRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
 
        var recordedBy = User.FindFirst(ClaimTypes.Name)!.Value;
 
        try
        {
            var response = await _nursingNoteService.UpdateNoteAsync(noteId, requestDto, recordedBy);
            return Ok(response);
        }
        catch (MediRecordsException ex) when (ex.Message.Contains("closed"))
        {
            return StatusCode(409, ex.Message);
        }
        catch (MediRecordsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, Constant.InternalError);
        }
    }
}