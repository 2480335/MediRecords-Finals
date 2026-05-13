using System;

namespace MediRecords.Dto.LabOrderDtos;

public class LabOrderFilterRequestDto
{
    public int? EncounterId { get; set; }
    public int? OrderedBy { get; set; }
    public string? TestJson { get; set; }
    public DateTime? OrderDate { get; set; }
    public bool? Status { get; set; }

}
