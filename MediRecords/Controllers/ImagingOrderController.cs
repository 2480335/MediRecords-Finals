using MediRecords.Domain.Entities;
using MediRecords.Dto.ImagingOrderDto;
using System.Security.Claims;
using MediRecords.Services.ImagingOrderServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ImagingOrderController : ControllerBase
    {
        private readonly IImagingOrderServices _service;

        public ImagingOrderController(IImagingOrderServices service)
        {
            _service = service;
        }
        /// <summary>
        /// Handles the HTTP POST request to create a new imaging order.
        /// This endpoint validates the request, ensures the user is authorized, 
        /// and manages exception-to-status-code mapping.
        /// </summary>
        /// <param name="dto">The imaging order data transfer object containing request details.</param>
        /// <returns>
        /// A 201 Created response on success; 
        /// 400 Bad Request if validation fails; 
        /// 404 Not Found if the encounter is missing; 
        /// 409 Conflict if the encounter is closed; 
        /// or 500 Internal Server Error for unhandled exceptions.
        /// </returns>
        [HttpPost("{EncounterId}")]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateAsync(int EncounterId, [FromBody] ImagingOrderRequestDto imagingDto)
        {
            if (imagingDto == null)
            {
                return BadRequest(Constant.RequestCannotBeNull);
            }
            try
            {
                var userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _service.AddAsync(EncounterId, imagingDto, userIdClaim);
                return StatusCode(StatusCodes.Status201Created, Constant.OrderCreated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, Constant.InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves imaging orders based on the provided filter criteria.
        /// </summary>
        /// <param name="filter">
        /// Query parameters used to filter imaging orders.
        /// </param>
        /// <returns>
        /// Returns a list of imaging orders if found; otherwise, a not found response.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = Constant.Physician)]
        [ProducesResponseType(typeof(List<ImagingOrder>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAllAsync([FromQuery] ImagingOrderFilterDto filter)
        {
            try
            {
                var results = await _service.GetAllAsync(filter);
                return Ok(results);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, Constant.InternalServerError);
            }
        }
    }
}
