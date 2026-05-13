using System.Security.Claims;
using System.Text;
using MediRecords.Dto.FollowUpDtos;
using MediRecords.Repository.FollowUpRepository;
using MediRecords.Services.FollowUpService;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FollowUpController : ControllerBase
    {
        IFollowUpService _followUpService;
        public FollowUpController(IFollowUpService followUpService)
        {
            _followUpService = followUpService;
        }

        [HttpPost("{encounterId}")]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddFollowUp(int encounterId, FollowUpCreateRequestDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                var id = await _followUpService.CreateFollowUpAsync(encounterId, dto, userId);
                return Created("", new FollowUpCreateResponseDto{ FollowupId = id });
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch(MediRecordsException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFollowUps(
            [FromQuery] int? patientId,
            [FromQuery] int? encounterId)
        {
            try
            {
                var result = await _followUpService.GetFollowUpsAsync(patientId, encounterId);
                if (result == null || !result.Any())
                {
                    return NotFound(new
                    {
                        message = "No immunization records found matching the provided filters."
                    });
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {
                    message = ex.Message
            });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFollowUpById(int id)
        {
            try
            {
                var result = await _followUpService.GetByIdAsync(id);
                return Ok(result); 
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(MediRecordsException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
