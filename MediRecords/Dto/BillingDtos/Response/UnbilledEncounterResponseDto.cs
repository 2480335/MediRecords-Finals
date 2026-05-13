namespace MediRecords.Dto.BillingDtos.Response;

public class UnbilledEncounterResponseDto
{
    public int EncounterId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalChargeAmount { get; set; }
    public int UnbilledChargeCount { get; set; }
}