using System;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.EncounterRepo;

public class EncounterRepository : IEncounterRepository
{
    private readonly MediRecordsDbContext _context;

    public EncounterRepository(MediRecordsDbContext context)
    {
        _context = context;
    }
    public async Task<Encounter?> GetByIdAsync(int encounterId)
    {
        return await _context.Encounters
            .Include( e => e.PatientIdNavigation)
            .Include( e => e.ProviderIdNavigation)
            .Include ( e => e.SOAPNotes)
            .Include ( e => e.VitalSigns )
            .Include ( e => e.NursingNotes )
            .Include ( e => e.LabOrders )
            .Include ( e => e.Prescriptions )
            .FirstOrDefaultAsync( e => e.EncounterId == encounterId);
    }

    public async Task<IEnumerable<Encounter>> GetByProviderAndDateAsync(int providerId, DateTime date)
    {
        return await _context.Encounters
            .Include(e => e.PatientIdNavigation)
            .Where(e => e.ProviderId == providerId && e.Date.Date == date.Date)
            .OrderBy( e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Encounter>> GetAllAsync()
    {
        return await _context.Encounters
            .Include(e => e.PatientIdNavigation)
            .AsNoTracking()
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<Encounter> UpdateStatusAsync(int encounterId, EncounterStatus newStatus)
    {
        var encounter = await _context.Encounters
              .FirstOrDefaultAsync( e => e.EncounterId == encounterId );

        if(encounter == null)
            return null;

        encounter.Status = newStatus;
        await _context.SaveChangesAsync();

        return encounter;
    }

    // ---------- Reports ----------

    public async Task<int> GetEncounterCountAsync(DateTime fromDate, DateTime toDate)
    {
        var fromDay = fromDate.Date;
        var toDay = toDate.Date;
        return await _context.Encounters
            .Where(e => e.Date >= fromDay && e.Date <= toDay)
            .CountAsync();
    }

    public async Task<IEnumerable<Encounter>> GetEncountersForDocumentationReportAsync(int? providerId, DateTime startDate, DateTime endDate)
    {
        var fromDay = startDate.Date;
        var toDay = endDate.Date;

        var query = _context.Encounters
            .Include(e => e.SOAPNotes)
            .Include(e => e.ProviderIdNavigation)
            .Where(e => e.Date >= fromDay && e.Date <= toDay)
            .AsQueryable();

        if (providerId.HasValue)
            query = query.Where(e => e.ProviderId == providerId.Value);

        return await query.ToListAsync();
    }

    public Task<int> GetTotalAppointmentsCountAsync(int? providerId, DateTime startDate, DateTime endDate)
        => CountAppointmentsAsync(providerId, startDate, endDate, status: null);

    public Task<int> GetNoShowCountAsync(int? providerId, DateTime startDate, DateTime endDate)
        => CountAppointmentsAsync(providerId, startDate, endDate, AppointmentStatus.NoShow);

    public Task<int> GetCancellationCountAsync(int? providerId, DateTime startDate, DateTime endDate)
        => CountAppointmentsAsync(providerId, startDate, endDate, AppointmentStatus.Cancelled);

    public Task<int> GetScheduledAppointmentsCount(int providerId, DateTime startDate, DateTime endDate)
        => CountAppointmentsAsync(providerId, startDate, endDate, status: null);

    public Task<int> GetCompletedEncountersCount(int providerId, DateTime startDate, DateTime endDate)
        => CountAppointmentsAsync(providerId, startDate, endDate, AppointmentStatus.Completed);

    public Task<int> GetCancelledEncountersCount(int providerId, DateTime startDate, DateTime endDate)
        => CountAppointmentsAsync(providerId, startDate, endDate, AppointmentStatus.Cancelled);

    public Task<int> GetNoShowAppointmentsCount(int providerId, DateTime startDate, DateTime endDate)
        => CountAppointmentsAsync(providerId, startDate, endDate, AppointmentStatus.NoShow);

    private async Task<int> CountAppointmentsAsync(int? providerId, DateTime startDate, DateTime endDate, AppointmentStatus? status)
    {
        var from = startDate.Date;
        var to = endDate.Date.AddDays(1).AddTicks(-1);

        var query = _context.Appointments
            .Where(a => a.DateTime >= from && a.DateTime <= to);

        if (providerId.HasValue)
            query = query.Where(a => a.ProviderId == providerId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query.CountAsync();
    }
}
