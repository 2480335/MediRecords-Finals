using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.MedicationListDtos;

public class MedicationListResponseDto
{
    public int MedId { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string DrugName { get; set; } = null!;
    public string? Dose { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Active";


    public static MedicationListResponseDto FromEntity(MedicationList entity)
    {
        return new MedicationListResponseDto
        {
            MedId = entity.MedId,
            PatientId = entity.PatientId,
            PatientName = entity.PatientIdNavigation?.Name ?? "Unknown",
            DrugName = entity.DrugName,
            Dose = entity.Dose,
            Frequency = entity.Frequency,
            Route = entity.Route,
            StartDate = DateOnly.FromDateTime(entity.StartDate),
            EndDate = DateOnly.FromDateTime(entity.EndDate ?? DateTime.MinValue),
            Status = entity.Status.ToString()
        };
    }
}
