using MediRecords.Dto.ImagingReportDto;
using MediRecords.Services.ImagingReportServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediRecords.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ImagingReportController : ControllerBase
    {
        private readonly IImagingReportServices _service;

        public ImagingReportController(IImagingReportServices service)
        {
            _service = service;

        }

        /// <summary>
        /// Endpoint for inserting the report to the database
        /// </summary>
        /// <param name="ImagingOrderID">This is the Imaging order id for which report is getting generated</param>
        /// <param name="dto">this will recieve file and findings and pass to the service layer</param>
        /// <returns></returns>
        [HttpPost("{ImagingOrderID}")]
        [Authorize(Roles = Constant.LabTech)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateAsync([FromRoute] int ImagingOrderID, [FromForm] ImagingReportRequestDto dto)
        {
            if (dto == null) return BadRequest(Constant.RequestCannotBeNull);

            try
            {
                await _service.CreateReportAsync(ImagingOrderID,dto);
                return StatusCode(StatusCodes.Status201Created, Constant.ReportCreated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, Constant.InternalServerError);
            }
        }
    }
}
