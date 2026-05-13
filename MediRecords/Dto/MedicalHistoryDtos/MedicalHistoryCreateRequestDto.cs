using System;

namespace MediRecords.Dto.MedicalHistoryDtos;

public class MedicalHistoryCreateRequestDto
{
    public required string Condition { get; set; }
    public string? Notes { get; set; }
}
