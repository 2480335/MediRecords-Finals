using MediRecords.Dto.DocumentDtos;
using MediRecords.Repository.DocumentRepository;
using MediRecords.Utility;

namespace MediRecords.Services.DocumentServices;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;

    public DocumentService(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DocumentUploadResponseDto> UploadDocumentAsync(DocumentUploadRequestDto request, int userId)
    {
        if (request == null)
            throw new MediRecordsException(Constant.RequestNull);

        if (request.PatientId <= 0)
            throw new MediRecordsException("Invalid patient ID.");

        if (request.EncounterId <= 0)
            throw new MediRecordsException("Invalid encounter ID.");

        if (request.File == null || request.File.Length == 0)
            throw new MediRecordsException("File is required.");

        if (string.IsNullOrWhiteSpace(request.DocType))
            throw new MediRecordsException("Document type is required.");

        return await _repository.UploadDocumentAsync(request, userId);
    }

    public async Task<DocumentDownloadResponseDto> DownloadDocumentAsync(int documentId, string timeZone = "UTC", int userId = 0)
    {
        if (documentId <= 0)
            throw new MediRecordsException("Invalid document ID.");

        if (string.IsNullOrWhiteSpace(timeZone))
            timeZone = "UTC";

        return await _repository.DownloadDocumentAsync(documentId, timeZone, userId);
    }

    public async Task<List<DocumentListResponseDto>> GetAllDocumentsAsync(DocumentFilterRequestDto filter, string timeZone = "UTC", int userId = 0)
    {
        if (filter == null)
            throw new MediRecordsException(Constant.RequestNull);

        if (string.IsNullOrWhiteSpace(timeZone))
            timeZone = "UTC";

        return await _repository.GetAllDocumentsAsync(filter, timeZone, userId);
    }
}
