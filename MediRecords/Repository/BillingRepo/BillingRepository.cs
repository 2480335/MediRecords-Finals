using System;
using Microsoft.EntityFrameworkCore;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.BillingRepo;

public class BillingRepository : IBillingRepository
{
    private readonly MediRecordsDbContext _context;
    public BillingRepository(MediRecordsDbContext context)
    {
        _context = context;
    }
    public async Task<VisitChargeRef> AddAsync(VisitChargeRef charge)
    {
        await _context.VisitChargeRefs.AddAsync(charge);
        await _context.SaveChangesAsync();

        // Reload with navigation properties for response
        return await _context.VisitChargeRefs
            .Include(v => v.ProcedureCodeNavigation)
            .FirstAsync(v => v.ChargeId == charge.ChargeId);
    }

    public async Task<bool> ChargeExistsAsync(int encounterId, int codeId)
    {
        return await _context.VisitChargeRefs.AnyAsync(v => v.EncounterId == encounterId && v.CodeId == codeId);
    }

    public async Task<bool> EncounterExistsAsync(int encounterId)
    {
        return await _context.Encounters.AnyAsync(e => e.EncounterId == encounterId);
    }

    public async Task<ProcedureCode?> GetProcedureCodeByIdAsync(int codeId)
    {
        return await _context.ProcedureCodes.FirstOrDefaultAsync(p => p.CodeId == codeId);
    }

    public async Task<VisitChargeRef?> GetChargeByIdAsync(int chargeId)
    {
        return await _context.VisitChargeRefs
            .Include(v => v.ProcedureCodeNavigation)
            .FirstOrDefaultAsync(v => v.ChargeId == chargeId);
    }

    public async Task<VisitChargeRef> UpdateAsync(VisitChargeRef charge)
    {
        _context.VisitChargeRefs.Update(charge);
        await _context.SaveChangesAsync();
        return charge;
    }

    public async Task<IEnumerable<VisitChargeRef>> GetChargesByEncounterIdAsync(int encounterId)
    {
        return await _context.VisitChargeRefs
            .Include(v => v.ProcedureCodeNavigation)
            .Where(v => v.EncounterId == encounterId)
            .OrderBy(v => v.ChargeId)
            .ToListAsync();
    }
    public async Task<(IEnumerable<Encounter> Encounters, int TotalCount)> GetUnbilledEncountersAsync(
        DateTime? fromDate, DateTime? toDate, int? providerId, int page, int pageSize)
    {
        // Base query — encounters that have at least one Unbilled charge
        var query = _context.Encounters
            .Include(e => e.PatientIdNavigation)
            .Include(e => e.ProviderIdNavigation)
            .Include(e => e.VisitChargeRefs)
            .Where(e => e.VisitChargeRefs.Any(v => !v.Status)); // false = Unbilled

        // Filter by date range
        if (fromDate.HasValue)
            query = query.Where(e => e.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(e => e.Date <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        // Filter by provider
        if (providerId.HasValue && providerId.Value > 0)
            query = query.Where(e => e.ProviderId == providerId.Value);

        // Total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var encounters = await query
            .OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (encounters, totalCount);
    }
     public async Task<List<VisitChargeRef>> GetChargesByIdsAsync(List<int> chargeIds)
    {
        return await _context.VisitChargeRefs
            .Where(v => chargeIds.Contains(v.ChargeId))
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(List<VisitChargeRef> charges)
    {
        _context.VisitChargeRefs.UpdateRange(charges);
        await _context.SaveChangesAsync();
    }

    public async Task AddAuditLogAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<VisitChargeRef>> GetChargesForExportAsync(
        string status, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.VisitChargeRefs
            .Include(v => v.ProcedureCodeNavigation)
            .Include(v => v.EncounterIdNavigation)
                .ThenInclude(e => e!.PatientIdNavigation)
            .AsQueryable();

        // Filter by status
        query = status.ToLower() switch
        {
            "unbilled" => query.Where(v => !v.Status),  // false = Unbilled
            "billed"   => query.Where(v => v.Status),   // true  = Billed
            _          => query                          // All
        };

        // Filter by date range on encounter date
        if (fromDate.HasValue)
            query = query.Where(v => v.EncounterIdNavigation!.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(v =>
                v.EncounterIdNavigation!.Date <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        return await query
            .OrderBy(v => v.EncounterIdNavigation!.Date)
            .ToListAsync();
    }
}