using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.ImmunizationRepository;

public interface IImmunizationRepository
{
    Task AddAsync(Immunization immunization);
    Task<Immunization?> GetByIdAsync(int immunizationId);
    Task<IEnumerable<Immunization>> GetAsync(int? patientId, string? patientName, string? vaccine, bool? status);
}
