using System;
using MediRecords.Domain.Entities;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace MediRecords.Repository.ProblemListRepository;

public class ProblemListRepository : IProblemListRepository
{
    private readonly MediRecordsDbContext _context;
    public ProblemListRepository(MediRecordsDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(ProblemList problem)
    {
        await _context.ProblemLists.AddAsync(problem);
        await _context.SaveChangesAsync();
    }
}
