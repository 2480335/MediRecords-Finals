using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.EncounterDtos.Response;

public class EncounterSummaryDto
{
    public int EncounterId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}