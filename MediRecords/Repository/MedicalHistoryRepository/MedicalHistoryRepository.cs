using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.MedicalHistoryRepository;

public class MedicalHistoryRepository : IMedicalHistoryRepository
{
    private readonly MediRecordsDbContext _context;
    public MedicalHistoryRepository(MediRecordsDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(MedicalHistory history)
    {
        await _context.MedicalHistories.AddAsync(history);
        await _context.SaveChangesAsync();
    }
}
