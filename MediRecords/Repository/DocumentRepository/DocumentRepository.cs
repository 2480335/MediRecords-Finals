using MediRecords.Dto.DocumentDtos;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Utility;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.DocumentRepository;

public class DocumentRepository : IDocumentRepository
{
    private readonly MediRecordsDbContext _dbContext;

    public DocumentRepository(MediRecordsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DocumentUploadResponseDto> UploadDocumentAsync(DocumentUploadRequestDto request, int userId)
    {
        try
        {
            // Validate patient exists
            var patient = await _dbContext.Patients
                .FirstOrDefaultAsync(p => p.PatientId == request.PatientId);
            
            if (patient == null)
                throw new MediRecordsException($"Patient with ID {request.PatientId} not found.");

            // Validate encounter exists
            var encounter = await _dbContext.Encounters
                .FirstOrDefaultAsync(e => e.EncounterId == request.EncounterId);
            
            if (encounter == null)
                throw new MediRecordsException($"Encounter with ID {request.EncounterId} not found.");

            // Validate file
            if (request.File == null || request.File.Length == 0)
                throw new MediRecordsException("File is required.");

            if (request.File.Length > 10 * 1024 * 1024) // 10MB max
                throw new MediRecordsException("File size cannot exceed 10MB.");

            // Validate file type and get MIME type
            var allowedMimeTypes = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" }
            };

            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
            
            if (!allowedMimeTypes.ContainsKey(fileExtension))
                throw new MediRecordsException("Only PDF, JPG, PNG files are allowed.");

            var mimeType = allowedMimeTypes[fileExtension];

            // Validate DocType
            if (!Enum.TryParse<DocumentType>(request.DocType, ignoreCase: true, out var docType))
                throw new MediRecordsException("Invalid document type. Allowed types: Referral, Consent, Report, Photo.");

            // Read file into byte array
            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await request.File.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            // Create database record with file stream
            var document = new Document
            {
                PatientID = request.PatientId,
                EncounterID = request.EncounterId,
                DocType = docType,
                FileName = request.File.FileName,
                FileType = mimeType,
                FileData = fileData,
                UploadedBy = userId,
                UploadedDate = DateTime.UtcNow,
                Status = DocumentStatus.Active,
                ProviderOnlyVisibility = request.ProviderOnlyVisibility
            };

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            // Log to AuditLog
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = "UPLOAD",
                Resource = $"Document - {document.FileName} (Type: {docType})",
                TimeStamp = DateTime.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();

            // Get uploader user name
            var uploader = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var uploaderName = uploader?.Name ?? "Unknown";

            return new DocumentUploadResponseDto
            {
                DocumentId = document.DocumentID,
                PatientId = document.PatientID,
                EncounterId = document.EncounterID,
                DocType = document.DocType.ToString(),
                FileName = document.FileName ?? string.Empty,
                FileType = document.FileType ?? string.Empty,
                UploadedBy = uploaderName,
                UploadedDate = document.UploadedDate,
                Status = document.Status.ToString(),
                Message = "Document uploaded successfully"
            };
        }
        catch (MediRecordsException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new MediRecordsException($"Database error while uploading document: {ex.InnerException?.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MediRecordsException($"Error uploading document: {ex.Message}", ex);
        }
    }

    public async Task<DocumentDownloadResponseDto> DownloadDocumentAsync(int documentId, string timeZone = "UTC", int userId = 0)
    {
        try
        {
            // Retrieve document from database
            var document = await _dbContext.Documents
                .FirstOrDefaultAsync(d => d.DocumentID == documentId && d.Status == DocumentStatus.Active);

            if (document == null)
                throw new MediRecordsException($"Document with ID {documentId} not found or has been deleted.");

            if (document.FileData == null || document.FileData.Length == 0)
                throw new MediRecordsException("Document file data is corrupted or missing.");

            // Log to AuditLog if userId is provided
            if (userId > 0)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "DOWNLOAD",
                    Resource = $"Document - {document.FileName} (ID: {documentId})",
                    TimeStamp = DateTime.UtcNow
                };
                _dbContext.AuditLogs.Add(auditLog);
                await _dbContext.SaveChangesAsync();
            }

            // Convert UTC time to user's timezone
            DateTime uploadedDateLocal = document.UploadedDate;
            try
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                uploadedDateLocal = TimeZoneInfo.ConvertTime(document.UploadedDate, TimeZoneInfo.Utc, tzInfo);
            }
            catch (TimeZoneNotFoundException)
            {
                // If timezone is invalid, use local time
                uploadedDateLocal = document.UploadedDate.ToLocalTime();
            }

            return new DocumentDownloadResponseDto
            {
                DocumentId = document.DocumentID,
                FileName = document.FileName ?? "document",
                FileType = document.FileType ?? "application/octet-stream",
                FileData = document.FileData,
                UploadedDate = uploadedDateLocal
            };
        }
        catch (MediRecordsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MediRecordsException($"Error downloading document: {ex.Message}", ex);
        }
    }

    public async Task<List<DocumentListResponseDto>> GetAllDocumentsAsync(DocumentFilterRequestDto filter, string timeZone = "UTC", int userId = 0)
    {
        try
        {
            // Build query with filters
            var query = _dbContext.Documents
                .Where(d => d.Status == DocumentStatus.Active)
                .AsQueryable();

            // Filter by PatientID
            if (filter.PatientId.HasValue && filter.PatientId > 0)
                query = query.Where(d => d.PatientID == filter.PatientId);

            // Filter by EncounterID
            if (filter.EncounterId.HasValue && filter.EncounterId > 0)
                query = query.Where(d => d.EncounterID == filter.EncounterId);

            // Filter by specific UploadedDate (entire day)
            if (filter.UploadedDate.HasValue)
            {
                var dateStart = filter.UploadedDate.Value.Date;
                var dateEnd = dateStart.AddDays(1);
                query = query.Where(d => d.UploadedDate >= dateStart && d.UploadedDate < dateEnd);
            }

            // Filter by UploadedDateFrom
            if (filter.UploadedDateFrom.HasValue)
                query = query.Where(d => d.UploadedDate >= filter.UploadedDateFrom);

            // Filter by UploadedDateTo
            if (filter.UploadedDateTo.HasValue)
            {
                var dateEnd = filter.UploadedDateTo.Value.AddDays(1); // Include entire day
                query = query.Where(d => d.UploadedDate < dateEnd);
            }

            // Execute query and get documents
            var documents = await query
                .Include(d => d.PatientIdNavigation)
                .ToListAsync();

            // Log to AuditLog if userId is provided
            if (userId > 0)
            {
                var filterDescription = $"Documents retrieved - ";
                if (filter.PatientId.HasValue)
                    filterDescription += $"PatientID: {filter.PatientId}, ";
                if (filter.EncounterId.HasValue)
                    filterDescription += $"EncounterID: {filter.EncounterId}, ";
                if (filter.UploadedDate.HasValue)
                    filterDescription += $"UploadedDate: {filter.UploadedDate:yyyy-MM-dd}";

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "SEARCH_DOCUMENTS",
                    Resource = filterDescription.TrimEnd().TrimEnd(','),
                    TimeStamp = DateTime.UtcNow
                };
                _dbContext.AuditLogs.Add(auditLog);
                await _dbContext.SaveChangesAsync();
            }

            // Convert to response DTOs with timezone conversion
            var responseDtos = new List<DocumentListResponseDto>();
            foreach (var doc in documents)
            {
                // Convert UTC time to user's timezone
                DateTime uploadedDateLocal = doc.UploadedDate;
                try
                {
                    var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    uploadedDateLocal = TimeZoneInfo.ConvertTime(doc.UploadedDate, TimeZoneInfo.Utc, tzInfo);
                }
                catch (TimeZoneNotFoundException)
                {
                    uploadedDateLocal = doc.UploadedDate.ToLocalTime();
                }

                // Get uploader name
                var uploader = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == doc.UploadedBy);
                var uploaderName = uploader?.Name ?? "Unknown";

                responseDtos.Add(new DocumentListResponseDto
                {
                    DocumentId = doc.DocumentID,
                    PatientId = doc.PatientID,
                    EncounterId = doc.EncounterID,
                    DocType = doc.DocType.ToString(),
                    FileName = doc.FileName ?? string.Empty,
                    UploadedBy = uploaderName,
                    UploadedDate = DateOnly.FromDateTime(uploadedDateLocal),
                    Status = doc.Status.ToString()
                });
            }

            return responseDtos.OrderByDescending(d => d.UploadedDate).ToList();
        }
        catch (MediRecordsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MediRecordsException($"Error retrieving documents: {ex.Message}", ex);
        }
    }
}
