namespace MediRecords.Dto.EncounterDtos.Response;

public class EncounterLookupDto
{
    public int EncounterId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}
