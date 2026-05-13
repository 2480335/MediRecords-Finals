using MediRecords.Domain.Entities;
using MediRecords.Dto.LabResultDtos;

namespace MediRecords.Repository.LabResultRepository;

public interface ILabResultRepository
{
    Task<LabResult> CreateLabResultAsync(LabResult labResult);
    Task<LabResult?> GetLabResultByIdAsync(int resultId);
    Task<IEnumerable<LabResult>> GetLabResultsByLabOrderIdAsync(int labOrderId);
    Task<IEnumerable<LabResult>> GetLabResultsAsync(LabResultRequestDto filter);
}
