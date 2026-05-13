using MediRecords.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.PrescriptionRepository;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly MediRecordsDbContext _context;

    public PrescriptionRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetPrescriptionCountAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Prescriptions
            .Where(p => p.CreatedDate >= fromDate && p.CreatedDate <= toDate)
            .CountAsync();
    }
}