namespace MediRecords.Dto.ReportsDtos;

public class NoShowCancellationReportDto
{
    public int? ProviderId { get; set; }
    public string ProviderName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalAppointments { get; set; }
    public int NoShows { get; set; }
    public int Cancellations { get; set; }
    public decimal NoShowPercentage { get; set; }
    public decimal CancellationPercentage { get; set; }
}
