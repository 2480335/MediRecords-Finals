using MediRecords.Domain.Entities;
using MediRecords.Dto.LabOrderDtos;
using MediRecords.Repository.LabOrderRepository;
using MediRecords.Repository.EncounterRepo;
using MediRecords.Services.AuthServices;
using MediRecords.Domain.Enums;

namespace MediRecords.Services.LabOrderServices;

public class LabOrderService : ILabOrderService
{
    private readonly ILabOrderRepository _labOrderRepository;
    private readonly IEncounterRepository _encounterRepository;
    private readonly IAuthService _authService;

    public LabOrderService(
        ILabOrderRepository labOrderRepository,
        IEncounterRepository encounterRepository,
        IAuthService authService)
    {
        _labOrderRepository = labOrderRepository;
        _encounterRepository = encounterRepository;
        _authService = authService;
    }

    public async Task<LabOrderResponseDto> CreateLabOrderAsync(LabOrderCreateRequestDto requestDto, int providerId)
    {
        if (requestDto == null)
            throw new ArgumentNullException(nameof(requestDto));

        if (requestDto.EncounterId <= 0)
            throw new ArgumentException("EncounterId is required.");

        if (string.IsNullOrWhiteSpace(requestDto.TestJson))
            throw new ArgumentException("TestJson is required.");

        
        if (requestDto.OrderDate.HasValue && requestDto.OrderDate.Value.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("OrderDate cannot be in the past.");

        var encounter = await _encounterRepository.GetByIdAsync(requestDto.EncounterId);
        if (encounter == null)
            throw new KeyNotFoundException($"Encounter with ID {requestDto.EncounterId} not found.");

        if (encounter.Status != EncounterStatus.Open)
            throw new InvalidOperationException($"Lab orders cannot be created for encounters with status {encounter.Status}.");

        var labOrder = new LabOrder
        {
            EncounterId = requestDto.EncounterId,
            OrderedBy = providerId,
            TestJson = requestDto.TestJson,
            OrderDate = requestDto.OrderDate ?? DateTime.UtcNow,
            Status = false  
        };

        var createdLabOrder = await _labOrderRepository.CreateLabOrderAsync(labOrder);
        await _authService.SaveAuditLog(providerId, $"CREATE_LABORDER:{createdLabOrder.LabOrderId}");
        return LabOrderResponseDto.FromEntity(createdLabOrder);
    }

    public async Task<LabOrderResponseDto?> GetLabOrderByIdAsync(int labOrderId)
    {
        var labOrder = await _labOrderRepository.GetLabOrderByIdAsync(labOrderId);
        return labOrder != null ? LabOrderResponseDto.FromEntity(labOrder) : null;
    }

    public async Task<IEnumerable<LabOrderResponseDto>> GetLabOrdersByEncounterIdAsync(int encounterId)
    {
        var labOrders = await _labOrderRepository.GetLabOrdersByEncounterIdAsync(encounterId);
        return labOrders.Select(LabOrderResponseDto.FromEntity);
    }

    public async Task<IEnumerable<LabOrderResponseDto>> GetLabOrdersAsync(LabOrderFilterRequestDto filter)
    {
        filter ??= new LabOrderFilterRequestDto();
        var labOrders = await _labOrderRepository.GetLabOrdersAsync(filter);
        return labOrders.Select(LabOrderResponseDto.FromEntity);
    }

    public async Task<LabOrderResponseDto> UpdateLabOrderStatusAsync(int labOrderId, bool status)
    {
        var labOrder = await _labOrderRepository.GetLabOrderByIdAsync(labOrderId);
        if (labOrder == null)
            throw new KeyNotFoundException($"Lab order with ID {labOrderId} not found.");

        labOrder.Status = status;
        var updatedLabOrder = await _labOrderRepository.UpdateLabOrderAsync(labOrder);
        return LabOrderResponseDto.FromEntity(updatedLabOrder);
    }
}
