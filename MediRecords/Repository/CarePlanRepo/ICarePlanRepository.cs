using System;
using MediRecords.Domain.Entities;
namespace MediRecords.Repository.CarePlanRepo;

public interface ICarePlanRepository
{
    Task<bool> PatientExistsAsync(int patientId);
    Task<CarePlan> AddAsync(CarePlan carePlan);
    Task<CarePlan?> GetByIdAsync(int carePlanId);
    Task<CarePlan?> GetActiveByPatientAsync(int patientId);
    Task<CarePlan> UpdateAsync(CarePlan carePlan);
    Task<IEnumerable<CarePlan>> GetAsync(
        int? patientId,
        string? patientName,
        bool? status);
}
