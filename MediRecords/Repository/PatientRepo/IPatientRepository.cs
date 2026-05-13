using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.PatientRepo;

public interface IPatientRepository
{
    Task<Patient> AddAsync(Patient patient);
    Task<string> GenerateMrnAsync();
    Task<bool> IsDuplicateAsync(string mrn, string? phone, DateOnly DOB);
    Task<Patient?> GetByIdWithDetailsAsync(int patientId);
    Task<Patient?> GetByIdAsync(int patientId);
    Task UpdateAsync(Patient patient);
    Task<IEnumerable<Patient>> GetAllAsync();
}
