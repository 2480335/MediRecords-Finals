using System;
using MediRecords.Domain.Entities;
namespace MediRecords.Repository.SOAPNoteRepo;

public interface ISOAPNoteRepository
{
    Task<SOAPNote> AddAsync(SOAPNote note);
    Task<SOAPNote> UpdateAsync(SOAPNote note);
    Task<SOAPNote?> GetByEncounterIdAsync(int encounterId);
}
