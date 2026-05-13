using System;
using MediRecords.Dto.ProviderProductivityDtos;

namespace MediRecords.Repository.ProviderProductivity;

public interface IProviderProductivity
{
    Task<IEnumerable<ProviderProductivityDto>> GetProviderProductivityAsync(int? providerId, DateTime fromDate, DateTime toDate);
}
