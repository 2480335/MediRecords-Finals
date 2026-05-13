using System;

namespace MediRecords.Dto.LabOrderDtos;

public class LabOrderCreateRequestDto
{
    public int EncounterId { get; set; }
    public string? TestJson { get; set; }
    public DateTime? OrderDate { get; set; }
}
