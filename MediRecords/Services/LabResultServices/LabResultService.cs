using MediRecords.Domain.Entities;
using MediRecords.Dto.LabResultDtos;
using MediRecords.Repository.LabResultRepository;
using MediRecords.Repository.LabOrderRepository;

namespace MediRecords.Services.LabResultServices;

public class LabResultService : ILabResultService
{
    private readonly ILabResultRepository _labResultRepository;
    private readonly ILabOrderRepository _labOrderRepository;

    public LabResultService(ILabResultRepository labResultRepository, ILabOrderRepository labOrderRepository)
    {
        _labResultRepository = labResultRepository;
        _labOrderRepository = labOrderRepository;
    }

    public async Task<LabResultResponseDto> CreateLabResultAsync(LabResultRequestDto requestDto)
    {
        if (requestDto == null)
            throw new ArgumentNullException(nameof(requestDto));

        if (!requestDto.LabOrderId.HasValue)
            throw new ArgumentException("LabOrderId is required.");

        // Verify that the LabOrder exists
        var labOrder = await _labOrderRepository.GetLabOrderByIdAsync(requestDto.LabOrderId.Value);
        if (labOrder == null)
            throw new KeyNotFoundException($"LabOrder with ID {requestDto.LabOrderId.Value} does not exist.");

        var labResult = new LabResult
        {
            LabOrderId = requestDto.LabOrderId.Value,
            ResultJson = requestDto.ResultJson,
            ResultDate = requestDto.ResultDate ?? DateTime.UtcNow,
            Status = requestDto.Status ?? false  // false = Final, true = Corrected
        };

        var createdLabResult = await _labResultRepository.CreateLabResultAsync(labResult);
        return LabResultResponseDto.FromEntity(createdLabResult);
    }

    public async Task<LabResultResponseDto?> GetLabResultByIdAsync(int resultId)
    {
        var labResult = await _labResultRepository.GetLabResultByIdAsync(resultId);
        return labResult != null ? LabResultResponseDto.FromEntity(labResult) : null;
    }

    public async Task<IEnumerable<LabResultResponseDto>> GetLabResultsByLabOrderIdAsync(int labOrderId)
    {
        var labResults = await _labResultRepository.GetLabResultsByLabOrderIdAsync(labOrderId);
        return labResults.Select(LabResultResponseDto.FromEntity);
    }

    public async Task<IEnumerable<LabResultResponseDto>> GetLabResultsAsync(LabResultRequestDto filter)
    {
        filter ??= new LabResultRequestDto();
        var labResults = await _labResultRepository.GetLabResultsAsync(filter);
        return labResults.Select(LabResultResponseDto.FromEntity);
    }
}
