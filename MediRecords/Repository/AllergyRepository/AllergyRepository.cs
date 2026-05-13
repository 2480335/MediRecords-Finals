using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.AllergyRepository;

public class AllergyRepository : IAllergyRepository
{
    private readonly MediRecordsDbContext _context;
    public AllergyRepository(MediRecordsDbContext  context)
    {
        _context = context;
    }
    public async Task AddAsync(Allergy allergy)
    {
        await _context.Allergies.AddAsync(allergy);
        await _context.SaveChangesAsync();
    }
}
