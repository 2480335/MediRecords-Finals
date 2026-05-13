using System;

namespace MediRecords.Dto.MedicationListDtos;

public class MedicationListRequestDto
{
    public int? PatientId { get; set; }
    public int? MedId { get; set; }
    public string? PatientName { get; set; }
    public string? DrugName { get; set; }
    public string? Dose { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; } // "Active" or "Inactive"
}
