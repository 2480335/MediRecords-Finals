using System;

using Microsoft.EntityFrameworkCore;
using MediRecords.Domain.Entities;


namespace MediRecords.Repository.CarePlanRepo;

public class CarePlanRepository : ICarePlanRepository
{
    private readonly MediRecordsDbContext _context;

    public CarePlanRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<bool> PatientExistsAsync(int patientId)
    {
        return await _context.Patients
            .AnyAsync(p => p.PatientId == patientId);
    }

    public async Task<CarePlan> AddAsync(CarePlan carePlan)
    {
        await _context.CarePlans.AddAsync(carePlan);
        await _context.SaveChangesAsync();
        return carePlan;
    }

    public async Task<CarePlan?> GetByIdAsync(int carePlanId)
    {
        return await _context.CarePlans
            .Include(cp => cp.PatientIdNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.CarePlanId == carePlanId);

    }

    public async Task<IEnumerable<CarePlan>> GetAsync(int? patientId, string? patientName, bool? status)
    {
        IQueryable<CarePlan> query = _context.CarePlans
            .Include(cp => cp.PatientIdNavigation)
            .AsNoTracking();

        if (patientId.HasValue)
        {
            query = query.Where(cp => cp.PatientId == patientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(patientName))
        {
            query = query.Where(cp => cp.PatientIdNavigation!.Name.Contains(patientName)); 
        }

        if (status.HasValue)
        {
            query = query.Where(cp => cp.Status == status.Value);
        }

        return await query
            .OrderByDescending(cp => cp.CarePlanId)
            .ToListAsync();
    }
}
