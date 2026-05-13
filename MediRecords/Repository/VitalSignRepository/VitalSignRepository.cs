using MediRecords.Models;
using MediRecords.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.VitalSignRepository;

public class VitalSignRepository : IVitalSignRepository
{
    private readonly MediRecordsDbContext _context;

    public VitalSignRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<VitalSign> AddAsync(VitalSign vitalSign)
    {
        
        await _context.VitalSign.AddAsync(vitalSign);
        await _context.SaveChangesAsync();
        return vitalSign;
    }

    public async Task<Encounter?> GetEncounterByIdAsync(int encounterId)
    {
        return await _context.Encounters.FindAsync(encounterId);
    }
}