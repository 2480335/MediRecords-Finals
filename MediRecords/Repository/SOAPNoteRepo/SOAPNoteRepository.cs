using System;
using MediRecords.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.SOAPNoteRepo;

public class SOAPNoteRepository : ISOAPNoteRepository
{
    private readonly MediRecordsDbContext _context;

    public SOAPNoteRepository(MediRecordsDbContext context)
    {
        _context = context;
    }
    public async Task<SOAPNote> AddAsync(SOAPNote note)
    {
        await _context.SOAPNotes.AddAsync(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<SOAPNote?> GetByEncounterIdAsync(int encounterId)
    {
        return await _context.SOAPNotes
            .FirstOrDefaultAsync<SOAPNote>(s => s.EncounterId == encounterId);
    }

    public async Task<SOAPNote> UpdateAsync(SOAPNote note)
    {
        _context.SOAPNotes.Update(note);
        await _context.SaveChangesAsync();
        return note;
    }
}