using System;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.PatientDtos;

public class PatientUpdateResponseDto
{
    public int PatientId { get; set; }
    public string MRN { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Gender { get; set; }
    public string? PhoneNo { get; set; }
    public string? AddressJSON { get; set; }
    public int? PrimaryProviderId { get; set; }
    public PatientStatus Status { get; set; }

}
public static class PatientUpdateResponseDtoExtension
{
    public static PatientUpdateResponseDto ToPatientUpdateResponse(this Patient patient)
    {
        return new PatientUpdateResponseDto
        {
            PatientId = patient.PatientId,
            MRN = patient.MRN,
            Name = patient.Name,
            Gender = patient.Gender,
            PhoneNo = patient.PhoneNo,
            AddressJSON = patient.AddressJSON,
            PrimaryProviderId = patient.PrimaryProviderId,
            Status = patient.Status
        };
    }
    
}
