using MediRecords.Dto.VitalSignDtos;

namespace MediRecords.Services.VitalSignServices;

public interface IVitalSignService
{
    Task<VitalSignCreateResponseDto> CreateVitalSignsAsync(VitalSignCreateRequestDto request, string NurseId);
}