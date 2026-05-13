namespace MediRecords.Dto.CarePlanDtos.Response;

public class CarePlanResponseDto
{
    public int CarePlanId { get; set; }
    public int PatientId { get; set; }
    public List<string> Goals { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; 
}