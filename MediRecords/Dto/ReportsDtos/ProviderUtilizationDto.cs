namespace MediRecords.Dto.ReportsDtos;

public class ProviderUtilizationDto
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; }
    public int ScheduledAppointments { get; set; }
    public int CompletedEncounters { get; set; }
    public int CancelledEncounters { get; set; }
    public int NoShowCount { get; set; }
    public decimal UtilizationRate { get; set; }
}