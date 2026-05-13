using System;
using MediRecords.Domain.Entities;
using MediRecords.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MediRecords.Repository.FollowUpRepository;

public class FollowUpRepository : IFollowUpRepository
{
    private  readonly MediRecordsDbContext _context;
    public FollowUpRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task AddFollowUpAsync(FollowUp followUp)
    {
        await _context.FollowUps.AddAsync(followUp);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<FollowUp>> GetAsync(int? patientId, int? encounterId)
    {
        IQueryable<FollowUp> query = _context.FollowUps
            .Include(f => f.EncounterIdNavigation)
                .ThenInclude(e => e.PatientIdNavigation)
            .AsNoTracking(); // Read-only optimization

        if (encounterId.HasValue)
        {
            query = query.Where(f => f.EncounterId == encounterId.Value);
        }

        if (patientId.HasValue)
        {
            query = query.Where(f =>
                f.EncounterIdNavigation != null &&
                f.EncounterIdNavigation.PatientId == patientId.Value);
        }

        return await query.OrderByDescending(f => f.RecommendedDate).ToListAsync();
    }

    public async Task<FollowUp?> GetByIdAsync(int followupId)
    {
        return await _context.FollowUps
            .Include(f => f.EncounterIdNavigation)
                .ThenInclude(e => e.PatientIdNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FollowupId == followupId);
    }
}
