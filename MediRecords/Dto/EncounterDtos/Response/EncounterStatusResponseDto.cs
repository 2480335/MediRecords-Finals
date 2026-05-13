using System;

namespace MediRecords.Dto.EncounterDtos.Response;

public class EncounterStatusResponseDto
{
    public int EncounterId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}