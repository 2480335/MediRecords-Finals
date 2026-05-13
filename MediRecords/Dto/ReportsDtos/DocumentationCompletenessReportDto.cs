namespace MediRecords.Dto.ReportsDtos;

public class IncompleteEncounterDto
{
    public int EncounterId { get; set; }
    public int ProviderId { get; set; }
    public string ProviderName { get; set; }
    public DateTime EncounterDate { get; set; }
    public string EncounterStatus { get; set; }
    public bool HasNoSoapNote { get; set; }
    public bool HasUnsignedSoapNote { get; set; }
    public List<string> MissingSections { get; set; } = new();
}

public class DocumentationCompletenessReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? ProviderId { get; set; }
    public int TotalEncounters { get; set; }
    public int IncompleteCount { get; set; }
    public List<IncompleteEncounterDto> IncompleteEncounters { get; set; } = new();
}
