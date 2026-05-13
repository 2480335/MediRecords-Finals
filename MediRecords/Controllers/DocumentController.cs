using Microsoft.AspNetCore.Mvc;
using MediRecords.Dto.DocumentDtos;
using MediRecords.Services.DocumentServices;
using MediRecords.Utility;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MediRecords.Domain.Enums;

namespace MediRecords.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentController(IDocumentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Upload a document for a patient
    /// </summary>
    /// <param name="request">Document upload request containing file and metadata</param>
    /// <returns>Document upload response with document details</returns>
    [HttpPost("upload")]
    [Authorize(Roles = nameof(UserRoleEnums.Admin) + "," + nameof(UserRoleEnums.Physician) + "," + nameof(UserRoleEnums.Nurse) + "," + nameof(UserRoleEnums.FrontDesk))]
    [ProducesResponseType(typeof(DocumentUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequestDto request)
    {
        try
        {
            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid or missing user ID" });

            var result = await _service.UploadDocumentAsync(request, userId);
            return Ok(result);
        }
        catch (MediRecordsException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpGet("download/{documentId}")]
    [Authorize(Roles = nameof(UserRoleEnums.Admin) + "," + nameof(UserRoleEnums.Physician) + "," + nameof(UserRoleEnums.Nurse) + "," + nameof(UserRoleEnums.FrontDesk))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadDocument(int documentId, [FromHeader(Name = "X-Timezone")] string? timeZone = "UTC")
    {
        try
        {
            // Use timezone from header, or default to UTC
            if (string.IsNullOrEmpty(timeZone))
                timeZone = "UTC";

            // Get current user ID from JWT claims for audit logging
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedUserId))
                userId = parsedUserId;

            var result = await _service.DownloadDocumentAsync(documentId, timeZone, userId);

            // Return file with MIME type for auto-download
            // Content-Disposition header tells browser to download instead of display
            return File(
                result.FileData,
                result.FileType,
                result.FileName,
                enableRangeProcessing: true);
        }
        catch (MediRecordsException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get all documents with filtering options
    /// </summary>
    /// <param name="patientId">Filter by Patient ID (optional)</param>
    /// <param name="encounterId">Filter by Encounter ID (optional)</param>
    /// <param name="uploadedDate">Filter by uploaded date (optional, full day match)</param>
    /// <param name="timeZone">User's timezone for date conversion (default: UTC)</param>
    /// <returns>List of documents matching the filter criteria</returns>
    [HttpGet("search")]
    [Authorize(Roles = nameof(UserRoleEnums.Admin) + "," + nameof(UserRoleEnums.Physician) + "," + nameof(UserRoleEnums.Nurse) + "," + nameof(UserRoleEnums.FrontDesk))]
    [ProducesResponseType(typeof(List<DocumentListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllDocuments(
        [FromQuery] int? patientId = null,
        [FromQuery] int? encounterId = null,
        [FromQuery] DateTime? uploadedDate = null,
        [FromHeader(Name = "X-Timezone")] string? timeZone = "UTC")
    {
        try
        {
            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid or missing user ID" });

            // Create filter request
            var filter = new DocumentFilterRequestDto
            {
                PatientId = patientId,
                EncounterId = encounterId,
                UploadedDate = uploadedDate
            };

            if (string.IsNullOrEmpty(timeZone))
                timeZone = "UTC";

            var result = await _service.GetAllDocumentsAsync(filter, timeZone, userId);
            return Ok(result);
        }
        catch (MediRecordsException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }
}
