using MediRecords.Dto.ReportsDtos;
using MediRecords.Repository.EncounterRepo;
using MediRecords.Repository.LabOrderRepository;
using MediRecords.Repository.PrescriptionRepository;
using MediRecords.Repository.UserRepo;
using MediRecords.Services.AuthServices;

namespace MediRecords.Services.ReportsServices;

public class ReportsService : IReportsService
{
    private readonly IEncounterRepository _encounterRepository;
    private readonly ILabOrderRepository _labOrderRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public ReportsService(
        IEncounterRepository encounterRepository,
        ILabOrderRepository labOrderRepository,
        IPrescriptionRepository prescriptionRepository,
        IUserRepository userRepository,
        IAuthService authService)
    {
        _encounterRepository = encounterRepository;
        _labOrderRepository = labOrderRepository;
        _prescriptionRepository = prescriptionRepository;
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<ClinicKpiReportDto> GetClinicKpiReportAsync(DateTime fromDate, DateTime toDate, int userId)
    {
        var visitCount = await _encounterRepository.GetEncounterCountAsync(fromDate, toDate);
        var labsOrdered = await _labOrderRepository.GetLabOrderCountAsync(fromDate, toDate);
        var rxIssued = await _prescriptionRepository.GetPrescriptionCountAsync(fromDate, toDate);

        // Log audit
        await _authService.SaveAuditLog(userId, $"GENERATE_CLINIC_KPIS_REPORT:{fromDate:yyyy-MM-dd}-{toDate:yyyy-MM-dd}");

        return new ClinicKpiReportDto
        {
            VisitCount = visitCount,
            LabsOrdered = labsOrdered,
            RxIssued = rxIssued
        };
    }

    public async Task<DocumentationCompletenessReportDto> GetDocumentationCompletenessReportAsync(int? providerId, DateTime startDate, DateTime endDate, int userId)
    {
        var encounters = await _encounterRepository.GetEncountersForDocumentationReportAsync(providerId, startDate, endDate);

        await _authService.SaveAuditLog(userId, $"GENERATE_DOCUMENTATION_COMPLETENESS_REPORT:{(providerId.HasValue ? providerId.ToString() : "ALL")}:{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}");

        var incompleteList = new List<IncompleteEncounterDto>();

        foreach (var encounter in encounters)
        {
            var hasNoSoapNote = !encounter.SOAPNotes.Any();
            var hasUnsignedSoapNote = encounter.SOAPNotes.Any(n => !n.Status);
            var missingSections = new List<string>();

            foreach (var note in encounter.SOAPNotes)
            {
                if (string.IsNullOrWhiteSpace(note.Objective))
                    missingSections.Add("Objective");
                if (string.IsNullOrWhiteSpace(note.Assessment))
                    missingSections.Add("Assessment");
                if (string.IsNullOrWhiteSpace(note.Plan))
                    missingSections.Add("Plan");
            }

            // Deduplicate section names (multiple notes could flag same section)
            missingSections = missingSections.Distinct().ToList();

            if (hasNoSoapNote || hasUnsignedSoapNote || missingSections.Count > 0)
            {
                incompleteList.Add(new IncompleteEncounterDto
                {
                    EncounterId = encounter.EncounterId,
                    ProviderId = encounter.ProviderId,
                    ProviderName = encounter.ProviderIdNavigation?.Name ?? string.Empty,
                    EncounterDate = encounter.Date.Date,
                    EncounterStatus = encounter.Status.ToString(),
                    HasNoSoapNote = hasNoSoapNote,
                    HasUnsignedSoapNote = hasUnsignedSoapNote,
                    MissingSections = missingSections
                });
            }
        }

        return new DocumentationCompletenessReportDto
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            ProviderId = providerId,
            TotalEncounters = encounters.Count(),
            IncompleteCount = incompleteList.Count,
            IncompleteEncounters = incompleteList
        };
    }

    public async Task<NoShowCancellationReportDto> GetNoShowCancellationReportAsync(int? providerId, DateTime startDate, DateTime endDate, int userId)
    {
        var total = await _encounterRepository.GetTotalAppointmentsCountAsync(providerId, startDate, endDate);
        var noShows = await _encounterRepository.GetNoShowCountAsync(providerId, startDate, endDate);
        var cancellations = await _encounterRepository.GetCancellationCountAsync(providerId, startDate, endDate);

        await _authService.SaveAuditLog(userId, $"GENERATE_NO_SHOW_CANCELLATION_REPORT:{(providerId.HasValue ? providerId.ToString() : "ALL")}:{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}");

        string providerName = "All Providers";
        if (providerId.HasValue)
        {
            var provider = await _userRepository.GetUserByIdAsync(providerId.Value);
            providerName = provider?.Name ?? string.Empty;
        }

        var noShowPct = total > 0 ? Math.Round((decimal)noShows / total * 100, 2) : 0m;
        var cancellationPct = total > 0 ? Math.Round((decimal)cancellations / total * 100, 2) : 0m;

        return new NoShowCancellationReportDto
        {
            ProviderId = providerId,
            ProviderName = providerName,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            TotalAppointments = total,
            NoShows = noShows,
            Cancellations = cancellations,
            NoShowPercentage = noShowPct,
            CancellationPercentage = cancellationPct
        };
    }

    public async Task<ProviderUtilizationDto> GetProviderUtilizationAsync(int providerId, DateTime startDate, DateTime endDate, int userId)
    {
        var scheduledAppointments = await _encounterRepository.GetScheduledAppointmentsCount(providerId, startDate, endDate);
        var completedEncounters = await _encounterRepository.GetCompletedEncountersCount(providerId, startDate, endDate);
        var cancelledEncounters = await _encounterRepository.GetCancelledEncountersCount(providerId, startDate, endDate);
        var noShowCount = await _encounterRepository.GetNoShowAppointmentsCount(providerId, startDate, endDate);

        await _authService.SaveAuditLog(userId, $"GENERATE_PROVIDER_UTILIZATION_REPORT:{providerId}:{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}");

        var provider = await _userRepository.GetUserByIdAsync(providerId);
        var providerName = provider?.Name ?? string.Empty;

        // Utilization rate = completed / total scheduled * 100
        var utilizationRate = scheduledAppointments > 0
            ? Math.Round((decimal)completedEncounters / scheduledAppointments * 100, 2)
            : 0m;

        return new ProviderUtilizationDto
        {
            ProviderId = providerId,
            ProviderName = providerName,
            ScheduledAppointments = scheduledAppointments,
            CompletedEncounters = completedEncounters,
            CancelledEncounters = cancelledEncounters,
            NoShowCount = noShowCount,
            UtilizationRate = utilizationRate
        };
    }
}