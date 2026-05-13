using System;

using MediRecords.Dto.CarePlanDtos.Request;
using MediRecords.Dto.CarePlanDtos.Response;

namespace MediRecords.Services.CarePlanServices;

public interface ICarePlanService
{
    Task<(bool Success, string Message, CarePlanResponseDto? Data)> CreateCarePlanAsync(
        CreateCarePlanRequestDto dto);
    Task<CarePlanDetailsDto> GetByIdAsync(int carePlanId);
    Task<IEnumerable<CarePlanDetailsDto>> GetCarePlansAsync(
        int? patientId,
        string? patientName,
        bool? status);
}
