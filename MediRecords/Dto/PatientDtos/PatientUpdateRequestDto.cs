using System;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.PatientDtos;

public class PatientUpdateRequestDto
{
    public string Name { get; set; } = null!;
    public DateOnly DOB { get; set; }
    public string? Gender { get; set; }
    public string PhoneNo { get; set; } = null!;
    public string? AddressJSON { get; set; }
    public int? PrimaryProviderId { get; set; }
    public PatientStatus Status { get; set; }
}
