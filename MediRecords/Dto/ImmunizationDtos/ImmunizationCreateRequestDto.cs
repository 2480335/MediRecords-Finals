using System;

namespace MediRecords.Dto.ImmunizationDtos;

public class ImmunizationCreateRequestDto
{
    public required string Vaccine { get; set; }
    public required string Dose { get; set; }
    public DateTime GivenDate { get; set; }
    public bool Status { get; set; }  // 0: Pending, 1: Administered
}
