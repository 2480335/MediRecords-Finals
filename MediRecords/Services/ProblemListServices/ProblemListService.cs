using System;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Dto.ProblemListDtos;
using MediRecords.Repositories;
using MediRecords.Repository.PatientRepo;
using MediRecords.Repository.ProblemListRepository;
using MediRecords.Services.AuthServices;
using MediRecords.Utility;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace MediRecords.Services.ProblemListServices;

public class ProblemListService : IProblemListService
{
    private readonly IPatientRepository _patientRepo;
    private readonly IProblemListRepository _problemListRepo;
    private readonly IAuthService _authService;
    public ProblemListService(IPatientRepository patientRepo, IProblemListRepository problemListRepo, IAuthService authService)
    {
        _patientRepo = patientRepo;
        _problemListRepo = problemListRepo;
        _authService = authService;
    }

    public async Task CreateProblemAsync(int patientId, ProblemCreateRequestDto dto, int userId)
    {
        
        if (string.IsNullOrWhiteSpace(dto.Diagnosis))
            throw new MediRecordsException(Constant.DiagnosisRequired);

        if (dto.StartDate > DateTime.UtcNow)
            throw new MediRecordsException(Constant.StartDateValidation);

        if (dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
            throw new MediRecordsException(Constant.EndDateValidation);

        // check if patient record exists or not
        var patient = await _patientRepo.GetByIdWithDetailsAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException(Constant.PatientMessages.PatientNotFound);

        // Check only assigned primary provider is accessing the patient 
        _authService.EnsurePrimaryProviderAccess(patient, userId);

        // Checks if problem list already exists in database (prevents duplicacy)
        var exists = patient.ProblemLists.Any(m => m.Diagnosis == dto.Diagnosis || (m.StartDate == dto.StartDate && m.EndDate == dto.EndDate));
        if (exists)
            throw new MediRecordsException(Constant.MedicalHistoryExists);

        var problem = new ProblemList
        {
            PatientId = patientId,
            Diagnosis = dto.Diagnosis.Trim(),
            Status = ProblemStatus.Active,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        await _problemListRepo.AddAsync(problem);

        await _authService.SaveAuditLog(
            userId,
            $"ADD_PROBLEM:{patientId}"
        );
    }
}
