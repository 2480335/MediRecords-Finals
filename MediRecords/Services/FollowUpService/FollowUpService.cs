using System;
using System.ClientModel.Primitives;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Dto.FollowUpDtos;
using MediRecords.Repository.EncounterRepo;
using MediRecords.Repository.FollowUpRepository;
using MediRecords.Services.AuthServices;
using MediRecords.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Newtonsoft.Json;

namespace MediRecords.Services.FollowUpService;

public class FollowUpService : IFollowUpService
{
    private readonly IFollowUpRepository _followUpRepo;
    IEncounterRepository _encounterRepo;
    private readonly IAuthService _authService;
    public FollowUpService(IFollowUpRepository followUpRepo, IEncounterRepository encounterRepo, IAuthService authService)
    {
        _followUpRepo = followUpRepo;
        _encounterRepo = encounterRepo;
        _authService = authService;
    }
    public async Task<int> CreateFollowUpAsync(int encounterId, FollowUpCreateRequestDto dto, int physicianId)
    {
        // Required fields validation
        if(string.IsNullOrWhiteSpace(dto.Notes))
        {
            throw new ArgumentException(Constant.RequiredFields);
        }
        if(dto.RecommendedDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException(Constant.FollowUpDate);
        }

        var encounter = await _encounterRepo.GetByIdAsync(encounterId);
        if(encounter == null)
        {
            throw new KeyNotFoundException(Constant.EncounterNotFound);
        }

        // Encounter status validation
        if(encounter.Status != EncounterStatus.Open)
        {
            throw new MediRecordsException(Constant.FollowUpConflict);
        }

        // Ensure physician owns encounter
        if(encounter.ProviderId != physicianId)
        {
            throw new MediRecordsException(Constant.FollowUpForbidden);
        }

        var followUp = new FollowUp
        {
            EncounterId = encounterId,
            RecommendedDate = dto.RecommendedDate,
            Notes = dto.Notes.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        // persist in the database
        await _followUpRepo.AddFollowUpAsync(followUp);

        // Audit log
        await _authService.SaveAuditLog(
            physicianId,
            "ADD_FOLLOWUP:{encounterId}"
        );
        return followUp.FollowupId;
    }

    public async Task<IEnumerable<FollowUpDetailsDto>> GetFollowUpsAsync(int? patientId, int? encounterId)
    {
        var list = await _followUpRepo.GetAsync(patientId, encounterId);

        return list.Select(f => new FollowUpDetailsDto
        {
            FollowupId = f.FollowupId,
            EncounterId = f.EncounterId,
            PatientId = f.EncounterIdNavigation!.PatientId,
            PatientName = f.EncounterIdNavigation.PatientIdNavigation!.Name,
            RecommendedDate = f.RecommendedDate,
            Notes = f.Notes,
            CreatedDate = f.CreatedDate
        });
    }

    public async Task<FollowUpDetailsDto> GetByIdAsync(int followupId)
    {
        if(followupId <= 0)
        {
            throw new ArgumentException("Invalid follow-up id");
        }

        var followUp = await _followUpRepo.GetByIdAsync(followupId);
        if (followUp == null)
        {
            throw new MediRecordsException("Follow‑up not found.");
        }

        return new FollowUpDetailsDto
        {
            FollowupId = followUp.FollowupId,
            EncounterId = followUp.EncounterId,
            PatientId = followUp.EncounterIdNavigation!.PatientId,
            PatientName = followUp.EncounterIdNavigation.PatientIdNavigation!.Name,
            RecommendedDate = followUp.RecommendedDate,
            Notes = followUp.Notes,
            CreatedDate = followUp.CreatedDate
        };
    }
}
