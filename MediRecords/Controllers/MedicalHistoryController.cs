using System.Security.Claims;
using MediRecords.Dto.MedicalHistoryDtos;
using MediRecords.Services.MedicalHistoryServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.Extensions.FileProviders;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly IMedicalHistoryService _medicalHistoryService;
        public MedicalHistoryController(IMedicalHistoryService medicalHistoryService)
        {
            _medicalHistoryService = medicalHistoryService;
        }

        /// <summary>
        /// Records the Medications of the patient.
        /// </summary>
        /// <param name="patientId">The numeric ID of the patient.</param>
        [HttpPost("{patientId}/medical-history")]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMedicalHistory(int patientId, MedicalHistoryCreateRequestDto dto)
        {
            if(!ModelState.IsValid || dto == null)
            {
                return BadRequest(ModelState);
            }
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try {
                await _medicalHistoryService.CreateMedicalHistoryAsync(patientId, dto, userId);
                return Ok(Constant.MedicalHistoryCreated);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch(MediRecordsException ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
