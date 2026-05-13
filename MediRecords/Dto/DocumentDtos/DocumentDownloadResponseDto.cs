namespace MediRecords.Dto.DocumentDtos;

public class DocumentDownloadResponseDto
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileType { get; set; } = null!; // MIME type
    public byte[] FileData { get; set; } = null!; // Binary file content
    public DateTime UploadedDate { get; set; } // Local time of upload
}
