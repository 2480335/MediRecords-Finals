using System;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.EncounterDtos.Request;

public class EncounterStatusUpdateDto
{
    public EncounterStatus Status { get; set; }
}