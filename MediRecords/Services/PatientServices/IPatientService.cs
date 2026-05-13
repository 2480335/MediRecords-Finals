using System;
using MediRecords.Dto.PatientDtos;

namespace MediRecords.Services.PatientServices;

public interface IPatientService
{
    Task<int> CreatePatientAsync(PatientCreateRequestDto requestDto, int frontDeskId);
    Task<PatientDetailsDto> GetPatientByIdAsync(int patientId);
    Task<PatientUpdateResponseDto> UpdatePatientAsync(int patientId, PatientUpdateRequestDto dto, int frontDeskId);
    Task<IEnumerable<PatientLookupDto>> GetAllPatientsAsync();
}
