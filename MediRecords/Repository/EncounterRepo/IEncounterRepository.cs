using System;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;

namespace MediRecords.Repository.EncounterRepo;

public interface IEncounterRepository
{
    Task<IEnumerable<Encounter>> GetByProviderAndDateAsync(int providerId, DateTime date);
    Task<Encounter?> GetByIdAsync(int encounterId);
    Task<Encounter> UpdateStatusAsync(int encounterId, EncounterStatus newStatus);
    Task<IEnumerable<Encounter>> GetAllAsync();

    // Reports
    Task<int> GetEncounterCountAsync(DateTime fromDate, DateTime toDate);
    Task<IEnumerable<Encounter>> GetEncountersForDocumentationReportAsync(int? providerId, DateTime startDate, DateTime endDate);

    Task<int> GetTotalAppointmentsCountAsync(int? providerId, DateTime startDate, DateTime endDate);
    Task<int> GetNoShowCountAsync(int? providerId, DateTime startDate, DateTime endDate);
    Task<int> GetCancellationCountAsync(int? providerId, DateTime startDate, DateTime endDate);

    Task<int> GetScheduledAppointmentsCount(int providerId, DateTime startDate, DateTime endDate);
    Task<int> GetCompletedEncountersCount(int providerId, DateTime startDate, DateTime endDate);
    Task<int> GetCancelledEncountersCount(int providerId, DateTime startDate, DateTime endDate);
    Task<int> GetNoShowAppointmentsCount(int providerId, DateTime startDate, DateTime endDate);
}
