using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.PatientDtos;

public class PatientCreateRequestDto
{
    public string Name { get; set; } = null!;
    public DateOnly DOB { get; set; }
    public string Gender { get; set; } = null!;
    public string PhoneNo { get; set; } = null!;
    public string? ContactInfo { get; set; }
    public string? AddressJSON { get; set; }
    public int? PrimaryProviderId { get; set; }
}
