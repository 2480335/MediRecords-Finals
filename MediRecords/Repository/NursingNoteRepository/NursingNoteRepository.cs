using MediRecords.Models;
using MediRecords.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.NursingNoteRepository;

public class NursingNoteRepository : INursingNoteRepository
{
    private readonly MediRecordsDbContext _context;

    public NursingNoteRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<NursingNote> AddAsync(NursingNote nursingNote)
    {
        if (nursingNote.NursingNoteId == 0)
        {
            nursingNote.NursingNoteId = await GetNextNursingNoteIdAsync();
        }

        _context.NursingNotes.Add(nursingNote);
        await _context.SaveChangesAsync();
        return nursingNote;
    }

    public async Task<NursingNote?> GetByIdAsync(int id)
    {
        return await _context.NursingNotes.FindAsync(id);
    }

    public async Task UpdateAsync(NursingNote nursingNote)
    {
        _context.NursingNotes.Update(nursingNote);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextNursingNoteIdAsync()
    {
        var maxId = await _context.NursingNotes.MaxAsync(x => (int?)x.NursingNoteId) ?? 0;
        return maxId + 1;
    }

    public async Task<Encounter?> GetEncounterByIdAsync(int encounterId)
    {
        return await _context.Encounters.FindAsync(encounterId);
    }
}
