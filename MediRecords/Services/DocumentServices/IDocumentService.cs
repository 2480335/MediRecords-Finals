using MediRecords.Dto.DocumentDtos;

namespace MediRecords.Services.DocumentServices;

public interface IDocumentService
{
    Task<DocumentUploadResponseDto> UploadDocumentAsync(DocumentUploadRequestDto request, int userId);
    Task<DocumentDownloadResponseDto> DownloadDocumentAsync(int documentId, string timeZone = "UTC", int userId = 0);
    Task<List<DocumentListResponseDto>> GetAllDocumentsAsync(DocumentFilterRequestDto filter, string timeZone = "UTC", int userId = 0);
}
