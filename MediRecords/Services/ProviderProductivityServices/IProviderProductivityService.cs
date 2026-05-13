using MediRecords.Dto.ProviderProductivityDtos;

namespace MediRecords.Services.ProviderProductivityServices;

public interface IProviderProductivityService
{
    Task<IEnumerable<ProviderProductivityDto>> GetProviderProductivityAsync(int? providerId, DateTime fromDate, DateTime toDate, int userId);
}