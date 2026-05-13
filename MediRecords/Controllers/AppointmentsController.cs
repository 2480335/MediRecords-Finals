using MediRecords.Domain.Enums;
using MediRecords.Dto.AppointmentDtos;
using MediRecords.Dto.AppointmentsDtos;
using MediRecords.Services.AppointmentsServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediRecords.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsService _appointmentService;

        public AppointmentsController(IAppointmentsService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost]
        [Authorize(Roles = Constant.FrontDesk)]
        [ProducesResponseType(typeof(AppointmentsResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> BookAppointment([FromBody] AppointmentsRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _appointmentService.BookAppointmentAsync(dto);
                return CreatedAtAction(nameof(BookAppointment), new { id = response.AppointmentId }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // Unified GET endpoint with flexible filters.
        // Physicians are automatically scoped to their own appointments —
        // any providerId supplied by a Physician client is overridden with
        // the caller's UserId. Admin / FrontDesk / Nurse / LabTech see all
        // (or whatever providerId they filter by).
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAppointments(
            [FromQuery] int? id,
            [FromQuery] int? patientId,
            [FromQuery] int? providerId,
            [FromQuery] string? date)
        {
            if (User.IsInRole(Constant.Physician))
            {
                var providerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(providerIdClaim) ||
                    !int.TryParse(providerIdClaim, out int callerId))
                {
                    return Unauthorized(new { message = "Invalid or missing provider token." });
                }

                // Force-scope to the logged-in physician. Ignore any
                // providerId the client tried to pass.
                providerId = callerId;
            }

            var response = await _appointmentService.GetAppointmentsAsync(id, patientId, providerId, date);

            if (id.HasValue && response.Count == 0)
                return NotFound(new { message = "Appointment not found" });

            return Ok(response);
        }

        [HttpPut("{id}/{status}")]
        [ProducesResponseType(typeof(AppointmentUpdateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAppointmentStatus(
            int id,
            AppointmentStatus status,
            [FromBody] AppointmentUpdateRequestDto request)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid appointment ID" });

            try
            {
                var response = await _appointmentService.UpdateAppointmentAsync(id, status, request);

                if (response == null)
                    return NotFound(new { message = "Appointment not found" });

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
