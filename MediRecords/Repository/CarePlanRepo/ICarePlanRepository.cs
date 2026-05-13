using System;
using MediRecords.Domain.Entities;
namespace MediRecords.Repository.CarePlanRepo;

public interface ICarePlanRepository
{
    Task<bool> PatientExistsAsync(int patientId);
    Task<CarePlan> AddAsync(CarePlan carePlan);
    Task<CarePlan?> GetByIdAsync(int carePlanId);
    Task<IEnumerable<CarePlan>> GetAsync(
        int? patientId,
        string? patientName,
        bool? status);
}
