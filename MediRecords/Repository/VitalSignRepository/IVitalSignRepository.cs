using MediRecords.Models;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.VitalSignRepository;

public interface IVitalSignRepository
{
    
    Task<VitalSign> AddAsync(VitalSign vitalSign); 
    Task<Encounter?> GetEncounterByIdAsync(int encounterId);
}