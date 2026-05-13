namespace MediRecords.Dto.DocumentDtos;

public class DocumentUploadResponseDto
{
    public int DocumentId { get; set; }
    public int PatientId { get; set; }
    public int EncounterId { get; set; }
    public string DocType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FileType { get; set; } = null!; // MIME type
    public string UploadedBy { get; set; } = null!;
    public DateTime UploadedDate { get; set; }
    public string Status { get; set; } = null!;
    public string Message { get; set; } = "Document uploaded successfully";
}
