using MediRecords.Dto.LabOrderDtos;

namespace MediRecords.Services.LabOrderServices;

public interface ILabOrderService
{
    Task<LabOrderResponseDto> CreateLabOrderAsync(LabOrderCreateRequestDto requestDto, int providerId);
    Task<LabOrderResponseDto?> GetLabOrderByIdAsync(int labOrderId);
    Task<IEnumerable<LabOrderResponseDto>> GetLabOrdersByEncounterIdAsync(int encounterId);
    Task<IEnumerable<LabOrderResponseDto>> GetLabOrdersAsync(LabOrderFilterRequestDto filter);
    Task<LabOrderResponseDto> UpdateLabOrderStatusAsync(int labOrderId, bool status);
}
