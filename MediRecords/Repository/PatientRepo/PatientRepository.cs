using System;
using MediRecords.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.PatientRepo;

public class PatientRepository : IPatientRepository
{
    private readonly MediRecordsDbContext _context;
    public PatientRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Patient> AddAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

   public async Task<string> GenerateMrnAsync()
    {
        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR PatientMrnSeq";

        if (command.Connection.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();
        var nextVal = Convert.ToInt64(result);

        return $"MR-{nextVal}";
    }

    public async Task<Patient?> GetByIdWithDetailsAsync(int patientId)
    {
        return await _context.Patients.Include(p => p.ProblemLists)
                        .Include(p => p.Allergies)
                        .Include(p => p.MedicalHistories)
                        .FirstOrDefaultAsync(p => p.PatientId == patientId);
    }

    public async Task<bool> IsDuplicateAsync(string mrn, string? phone, DateOnly DOB)
    {
        return await _context.Patients.AnyAsync(p => p.MRN == mrn || (!string.IsNullOrEmpty(phone) && p.PhoneNo == phone && p.DOB == DOB));
    }

    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        return await _context.Patients.FindAsync(patientId);
    }

    public async Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _context.Patients
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
