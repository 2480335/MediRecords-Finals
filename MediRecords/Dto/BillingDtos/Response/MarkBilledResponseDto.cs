namespace MediRecords.Dto.BillingDtos.Response;

public class MarkBilledResponseDto
{
    public List<int> MarkedAsBilled { get; set; } = new(); 
    public List<int> AlreadyBilled { get; set; } = new(); 
    public List<int> NotFound { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}