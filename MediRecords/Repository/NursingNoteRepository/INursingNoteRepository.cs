using MediRecords.Models;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.NursingNoteRepository;

public interface INursingNoteRepository
{
    Task<NursingNote> AddAsync(NursingNote nursingNote);
    Task<NursingNote?> GetByIdAsync(int id);
    Task UpdateAsync(NursingNote nursingNote);
    Task<int> GetNextNursingNoteIdAsync();
    Task<Encounter?> GetEncounterByIdAsync(int encounterId);
}
