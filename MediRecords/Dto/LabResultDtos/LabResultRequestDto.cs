using System;

namespace MediRecords.Dto.LabResultDtos;

public class LabResultRequestDto
{
    public int? LabOrderId { get; set; }
    public string? ResultJson { get; set; }
    public DateTime? ResultDate { get; set; }
    public bool? Status { get; set; }
}
