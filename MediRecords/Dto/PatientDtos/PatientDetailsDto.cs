using System;

namespace MediRecords.Dto.PatientDtos;

public class PatientDetailsDto
{
    public int PatientId { get; set; }
    public string MRN { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateOnly DOB { get; set; }
    public string? Gender { get; set; }
    public string PhoneNo { get; set; } = null!;
    public string? AddressJSON { get; set; }
    public string Status { get; set; } = null!;

    public IEnumerable<string> Problems { get; set; } = [];
    public IEnumerable<string> Allergies { get; set; } = [];
    public IEnumerable<string> MedicalHistory { get; set; } = [];
}
