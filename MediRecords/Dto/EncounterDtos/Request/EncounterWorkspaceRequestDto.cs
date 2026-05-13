using System;

namespace MediRecords.Dto.EncounterDtos.Request;

public class EncounterWorkspaceRequestDto
{
    public int ProviderId { get; set; }
    public DateTime? Date { get; set; }
}