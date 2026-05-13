using System;

namespace MediRecords.Dto.CarePlanDtos.Response;

public class CarePlanDetailsDto
{
    public int CarePlanId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public string GoalsJSON { get; set; } = null!;
    public string Instructions { get; set; } = null!;
    public bool Status { get; set; }
}
