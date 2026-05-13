using System;
using Humanizer;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.EncounterDtos.Response;

public class EncounterDetailDto
{
    public int EncounterId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}