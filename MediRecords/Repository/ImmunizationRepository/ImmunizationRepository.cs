using System;
using MediRecords.Domain.Entities;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.ImmunizationRepository;

public class ImmunizationRepository : IImmunizationRepository
{
    private readonly MediRecordsDbContext _context;
    public ImmunizationRepository(MediRecordsDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Immunization immunization)
    {
        await _context.Immunizations.AddAsync(immunization);
        await _context.SaveChangesAsync(); 
    }

    public async Task<IEnumerable<Immunization>> GetAsync(int? patientId, string? patientName, string? vaccine, bool? status)
    {
        IQueryable<Immunization> query = _context.Immunizations
            .Include(i => i.PatientIdNavigation)
            .AsNoTracking();

        if (patientId.HasValue)
        {
            query = query.Where(i => i.PatientId == patientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(patientName))
        {
            query = query.Where(i => i.PatientIdNavigation!.Name.Contains(patientName));
        }

        if (!string.IsNullOrWhiteSpace(vaccine))
        {
            query = query.Where(i => i.Vaccine == vaccine);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        return await query
            .OrderByDescending(i => i.GivenDate)
            .ToListAsync();
    }

    public async Task<Immunization?> GetByIdAsync(int immunizationId)
    {
        return await _context.Immunizations.OrderByDescending(e => e.GivenDate)
            .Include(i => i.PatientIdNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ImmunizationId == immunizationId);
    }
}
