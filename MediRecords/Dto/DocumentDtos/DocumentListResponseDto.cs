namespace MediRecords.Dto.DocumentDtos;

public class DocumentListResponseDto
{
    public int DocumentId { get; set; }
    public int PatientId { get; set; }
    public int EncounterId { get; set; }
    public string DocType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string UploadedBy { get; set; } = null!;
    public DateOnly UploadedDate { get; set; }
    public string Status { get; set; } = null!;
}
