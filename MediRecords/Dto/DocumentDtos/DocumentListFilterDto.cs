namespace MediRecords.Dto.DocumentDtos;

public class DocumentListFilterDto
{
    public int? PatientId { get; set; }
    public int? EncounterId { get; set; }
    public DateTime? UploadedDateFrom { get; set; }
    public DateTime? UploadedDateTo { get; set; }
}
