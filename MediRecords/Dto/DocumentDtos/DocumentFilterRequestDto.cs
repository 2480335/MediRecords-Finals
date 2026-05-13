namespace MediRecords.Dto.DocumentDtos;

public class DocumentFilterRequestDto
{
    public int? PatientId { get; set; }
    public int? EncounterId { get; set; }
    public DateTime? UploadedDate { get; set; } // Filter by specific date or use with UploadedDateFrom/To
    public DateTime? UploadedDateFrom { get; set; } // Filter documents uploaded after this date
    public DateTime? UploadedDateTo { get; set; } // Filter documents uploaded before this date
}
