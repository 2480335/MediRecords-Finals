using System;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ProviderProductivityDtos;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.ProviderProductivity;

public class ProviderProductivity : IProviderProductivity
{
    private readonly MediRecordsDbContext _context;

    public ProviderProductivity(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProviderProductivityDto>> GetProviderProductivityAsync(int? providerId, DateTime fromDate, DateTime toDate)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (providerId.HasValue)
            query = query.Where(u => u.UserId == providerId.Value);

        var result = await query.Select(u => new ProviderProductivityDto
        {
            ProviderId = u.UserId,
            ProviderName = u.Name,
            EncounterCount = _context.Encounters
                .AsNoTracking()
                .Count(e => e.ProviderId == u.UserId && e.Date >= fromDate && e.Date <= toDate),
            LabsOrdered = _context.LabOrders
                .AsNoTracking()
                .Count(lo => lo.OrderedBy == u.UserId && lo.OrderDate >= fromDate && lo.OrderDate <= toDate),
            PrescriptionsIssued = _context.Prescriptions
                .AsNoTracking()
                .Count(p => p.ProviderId == u.UserId && p.CreatedDate >= fromDate && p.CreatedDate <= toDate)
        }).ToListAsync();

        return result;
    }
}


