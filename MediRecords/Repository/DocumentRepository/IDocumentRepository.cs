using MediRecords.Dto.DocumentDtos;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.DocumentRepository;

public interface IDocumentRepository
{
    Task<DocumentUploadResponseDto> UploadDocumentAsync(DocumentUploadRequestDto request, int userId);
    Task<DocumentDownloadResponseDto> DownloadDocumentAsync(int documentId, string timeZone = "UTC", int userId = 0);
    Task<List<DocumentListResponseDto>> GetAllDocumentsAsync(DocumentFilterRequestDto filter, string timeZone = "UTC", int userId = 0);
}
