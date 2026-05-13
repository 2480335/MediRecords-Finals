using System.Security.Claims;
using MediRecords.Dto.ImmunizationDtos;
using MediRecords.Services.ImmunizationService;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ImmunizationController : ControllerBase
    {
        private readonly IImmunizationService _immunizationService;
        public ImmunizationController(IImmunizationService immunizationService)
        {
            _immunizationService = immunizationService;
        }

        [HttpPost("{patientId}")]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddImmunization(
            int patientId,
            [FromBody] ImmunizationCreateRequestDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                var id = await _immunizationService
                    .CreateImmunizationAsync(patientId, dto, userId);

                return Created("",
                    new ImmunizationCreateResponseDto { ImmunizationId = id }); 
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (MediRecordsException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpGet]
        [Authorize(Roles = Constant.Physician + "," + Constant.Nurse)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetImmunizations(
            [FromQuery] int? patientId,
            [FromQuery] string? patientName,
            [FromQuery] string? vaccine,
            [FromQuery] bool? status)
        {
            try
            {
                var result = await _immunizationService
                    .GetImmunizationsAsync(
                        patientId,
                        patientName,
                        vaccine,
                        status);

                if (result == null || !result.Any())
                {
                    return NotFound(new
                    {
                        message = "No immunization records found matching the provided filters."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = Constant.InternalError + ex.Message
                });
            }
        }


        [HttpGet("{id}")]
        [Authorize(Roles = Constant.Physician + "," + Constant.Nurse)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImmunizationById(int id)
        {
            try
            {
                var result = await _immunizationService.GetByIdAsync(id);
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
