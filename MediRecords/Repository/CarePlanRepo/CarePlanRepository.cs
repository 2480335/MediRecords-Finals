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

    /// <summary>
    /// Returns the patient's existing Active care plan (Status == false), if any.
    /// Returned with tracking enabled so callers can mutate and save.
    /// </summary>
    public async Task<CarePlan?> GetActiveByPatientAsync(int patientId)
    {
        return await _context.CarePlans
            .FirstOrDefaultAsync(cp => cp.PatientId == patientId && cp.Status == false);
    }

    public async Task<CarePlan> UpdateAsync(CarePlan carePlan)
    {
        _context.CarePlans.Update(carePlan);
        await _context.SaveChangesAsync();
        return carePlan;
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
