using MediRecords.Domain.Entities;
using MediRecords.Dto.LabOrderDtos;

namespace MediRecords.Repository.LabOrderRepository;

public interface ILabOrderRepository
{
    Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder);
    Task<LabOrder?> GetLabOrderByIdAsync(int labOrderId);
    Task<IEnumerable<LabOrder>> GetLabOrdersByEncounterIdAsync(int encounterId);
    Task<IEnumerable<LabOrder>> GetLabOrdersAsync(LabOrderFilterRequestDto filter);
    Task<LabOrder> UpdateLabOrderAsync(LabOrder labOrder);

    // Reports
    Task<int> GetLabOrderCountAsync(DateTime fromDate, DateTime toDate);
}
