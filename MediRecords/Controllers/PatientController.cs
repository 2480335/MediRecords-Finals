using System.Security.Claims;
using MediRecords.Domain.Entities;
using MediRecords.Dto.PatientDtos;
using MediRecords.Services.PatientServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        
        /// <summary>
        /// Registers a new patient into the MediRecords system.
        /// </summary>
        /// <param name="requestDto">The patient registration data transfer object containing patient details.</param>
        /// <returns>Return the Success or ErrorMessage</returns>
        [HttpPost]
        [Authorize(Roles = Constant.FrontDesk)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreatePatient([FromBody] PatientCreateRequestDto requestDto)
        {
            if(!ModelState.IsValid || requestDto == null)
            {
                return BadRequest(ModelState);
            }
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            try
            {
                var userId = int.Parse(userIdClaim.Value);

                var patientId = await _patientService.CreatePatientAsync(requestDto, userId);

                return Created("", new PatientCreateResponseDto { PatientId = patientId });
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

        /// <summary>
        /// Retrieves all patients for dropdown selection across forms.
        /// </summary>
        /// <returns>A list of PatientLookupDto objects (PatientId, Name, MRN).</returns>
        [HttpGet("all")]
        [Authorize(Roles = Constant.FrontDesk + "," + Constant.Physician + "," + Constant.Nurse + "," + Constant.Admin)]
        [ProducesResponseType(typeof(IEnumerable<PatientLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPatients()
        {
            try
            {
                var result = await _patientService.GetAllPatientsAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = Constant.InternalError });
            }
        }

        /// <summary>
        /// Fetches a specific patient's details by their unique ID.
        /// </summary>
        /// <param name="id">The numeric ID of the patient.</param>
        [HttpGet("{id}")]
        [Authorize(Roles = Constant.FrontDesk + "," + Constant.Physician + "," + Constant.Nurse)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPatientById(int id)
        {
            try
            {
                var result = await _patientService.GetPatientByIdAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Updates patient details by an FrontDesk.
        /// </summary>
        /// <param name="dto">User details to be updated by frontdesk</param>
        /// <returns>Returns updated patient information</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = Constant.FrontDesk)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePatient(
            int id,
            [FromBody] PatientUpdateRequestDto dto)
        {
            if (!ModelState.IsValid || dto == null)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var updatedPatient = await _patientService.UpdatePatientAsync(id, dto, userId);
                return Ok(updatedPatient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (MediRecordsException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
