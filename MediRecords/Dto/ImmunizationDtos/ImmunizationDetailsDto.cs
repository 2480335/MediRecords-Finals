using System;

namespace MediRecords.Dto.ImmunizationDtos;

public class ImmunizationDetailsDto
{
    public int ImmunizationId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public string Vaccine { get; set; } = null!;
    public string Dose { get; set; } = null!;
    public DateTime GivenDate { get; set; }
    public bool Status { get; set; }
}
