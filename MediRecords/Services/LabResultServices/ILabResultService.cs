using MediRecords.Dto.LabResultDtos;

namespace MediRecords.Services.LabResultServices;

public interface ILabResultService
{
    Task<LabResultResponseDto> CreateLabResultAsync(LabResultRequestDto requestDto);
    Task<LabResultResponseDto?> GetLabResultByIdAsync(int resultId);
    Task<IEnumerable<LabResultResponseDto>> GetLabResultsByLabOrderIdAsync(int labOrderId);
    Task<IEnumerable<LabResultResponseDto>> GetLabResultsAsync(LabResultRequestDto filter);
}
