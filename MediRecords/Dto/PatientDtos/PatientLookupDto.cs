namespace MediRecords.Dto.PatientDtos;

public class PatientLookupDto
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty;
    public DateOnly DOB { get; set; }
    public string? Gender { get; set; }
    public string PhoneNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
