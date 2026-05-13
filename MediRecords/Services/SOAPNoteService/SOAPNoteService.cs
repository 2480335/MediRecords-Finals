using System;
using MediRecords.Domain.Enums;
using MediRecords.Domain.Entities;
using MediRecords.Dto.SOAPNoteDtos.Request;
using MediRecords.Dto.SOAPNoteDtos.Response;
using MediRecords.Utility;
using Microsoft.IdentityModel.Tokens;
using MediRecords.Repository.SOAPNoteRepo;
using MediRecords.Repository.EncounterRepo;

namespace MediRecords.Services.SOAPNoteService;

public class SOAPNoteService : ISOAPNoteService
{
    private readonly ISOAPNoteRepository _soapRepo;
    private readonly IEncounterRepository _encounterRepo;

    public SOAPNoteService(
    ISOAPNoteRepository soapRepo,
    IEncounterRepository encounterRepo)
    {
        _soapRepo = soapRepo;
        _encounterRepo = encounterRepo;
    }
    public async Task<(bool Success, string Message, SOAPNoteResponseDto? Data)> SaveSOAPNoteAsync(int encounterId, SaveSOAPNoteRequestDto dto)
    {
        try
        {
            // Validate encounter ID
            if (encounterId <= 0)
                return (false, Constant.SOAPNoteMessages.InvalidEncounterId, null);

            // Validate HPI is provided
            if (string.IsNullOrWhiteSpace(dto.HPI))
                return (false, Constant.SOAPNoteMessages.HPIRequired, null);

            // Check encounter exists
            var encounter = await _encounterRepo.GetByIdAsync(encounterId);
            if (encounter == null)
                return (false, Constant.SOAPNoteMessages.EncounterNotFound, null);

            // Locked encounter cannot have SOAP note saved
            if (encounter.Status == EncounterStatus.Locked)
                return (false, Constant.SOAPNoteMessages.EncounterLocked, null);

            // Check if SOAP note already exists for this encounter
            var existing = await _soapRepo.GetByEncounterIdAsync(encounterId);

            if (existing != null)
            {
                // Cannot modify a signed & locked SOAP note
                if (!existing.Status) // false = Signed & Locked
                    return (false, Constant.SOAPNoteMessages.SOAPNoteLocked, null);

                // Update existing draft — pack UI fields into JSON columns
                existing.Subjective  = SOAPNoteHelper.SerializeSubjective(dto.HPI, dto.ROS);
                existing.Objective   = SOAPNoteHelper.SerializeObjective(dto.ExamFindings, dto.Observations);
                existing.Assessment  = dto.Assessment;
                existing.Plan        = dto.Plan;
                existing.Status      = dto.IsDraft;
                existing.CreatedDate = DateTime.UtcNow;

                var updated  = await _soapRepo.UpdateAsync(existing);
                var response = SOAPNoteHelper.ToResponseDto(updated);
                var msg      = dto.IsDraft ? Constant.SOAPNoteMessages.SOAPNoteSaved : Constant.SOAPNoteMessages.SOAPNoteSigned;
                return (true, msg, response);
            }

            // Create new SOAP note — pack UI fields into JSON columns
            var note = new SOAPNote
            {
                EncounterId  = encounterId,
                Subjective   = SOAPNoteHelper.SerializeSubjective(dto.HPI, dto.ROS),
                Objective    = SOAPNoteHelper.SerializeObjective(dto.ExamFindings, dto.Observations),
                Assessment   = dto.Assessment,
                Plan         = dto.Plan,
                Status       = dto.IsDraft,  // true = Draft, false = Sign & Lock
                CreatedDate  = DateTime.UtcNow
            };

            var saved   = await _soapRepo.AddAsync(note);
            var result  = SOAPNoteHelper.ToResponseDto(saved);
            var message = dto.IsDraft ? Constant.SOAPNoteMessages.SOAPNoteSaved : Constant.SOAPNoteMessages.SOAPNoteSigned;

            return (true, message, result);
        }
        catch (Exception)
        {
            return (false, Constant.SOAPNoteMessages.SomethingWentWrong, null);
        }
    }
}
