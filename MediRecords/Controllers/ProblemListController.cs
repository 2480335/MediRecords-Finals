using System.Security.Claims;
using MediRecords.Services.PatientServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediRecords.Utility;
using MediRecords.Dto.ProblemListDtos;
using MediRecords.Services.ProblemListServices;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProblemListController : ControllerBase
    {
        private readonly IProblemListService _problemListService;
        public ProblemListController(IProblemListService problemListService)
        {
            _problemListService = problemListService;
        }
        
        /// <summary>
        /// Records the problems of the patient.
        /// </summary>
        /// <param name="patientId">The numeric ID of the patient.</param>
        [HttpPost("{patientId}/problems")]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddProblem(
            int patientId,
            [FromBody] ProblemCreateRequestDto dto)
        {
            if(!ModelState.IsValid || dto == null)
            {
                return BadRequest(ModelState);
            }
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                await _problemListService.CreateProblemAsync(patientId, dto, userId);
                return Ok(Constant.ProblemCreated);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (MediRecordsException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
