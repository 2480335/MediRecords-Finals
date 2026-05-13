namespace MediRecords.Dto.ProviderProductivityDtos;

public class ProviderProductivityDto
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public int EncounterCount { get; set; }
    public int LabsOrdered { get; set; }
    public int PrescriptionsIssued { get; set; }
}