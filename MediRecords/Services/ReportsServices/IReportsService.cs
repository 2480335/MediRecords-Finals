using MediRecords.Dto.ReportsDtos;

namespace MediRecords.Services.ReportsServices;

public interface IReportsService
{
    Task<ClinicKpiReportDto> GetClinicKpiReportAsync(DateTime fromDate, DateTime toDate, int userId);
    Task<ProviderUtilizationDto> GetProviderUtilizationAsync(int providerId, DateTime startDate, DateTime endDate, int userId);
    Task<NoShowCancellationReportDto> GetNoShowCancellationReportAsync(int? providerId, DateTime startDate, DateTime endDate, int userId);
    Task<DocumentationCompletenessReportDto> GetDocumentationCompletenessReportAsync(int? providerId, DateTime startDate, DateTime endDate, int userId);
}