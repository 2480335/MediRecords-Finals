using MediRecords.Dto.ProviderProductivityDtos;
using MediRecords.Repository.ProviderProductivity;
using MediRecords.Services.AuthServices;

namespace MediRecords.Services.ProviderProductivityServices;

public class ProviderProductivityService : IProviderProductivityService
{
    private readonly IProviderProductivity _providerProductivityRepository;
    private readonly IAuthService _authService;

    public ProviderProductivityService(
        IProviderProductivity providerProductivityRepository,
        IAuthService authService)
    {
        _providerProductivityRepository = providerProductivityRepository;
        _authService = authService;
    }

    public async Task<IEnumerable<ProviderProductivityDto>> GetProviderProductivityAsync(int? providerId, DateTime fromDate, DateTime toDate, int userId)
    {
        var result = await _providerProductivityRepository.GetProviderProductivityAsync(providerId, fromDate, toDate);

        // Log audit
        string providerText = providerId.HasValue ? $"Provider:{providerId}" : "AllProviders";
        await _authService.SaveAuditLog(userId, $"GENERATE_PROVIDER_PRODUCTIVITY_REPORT:{providerText}:{fromDate:yyyy-MM-dd}-{toDate:yyyy-MM-dd}");

        return result;
    }
}