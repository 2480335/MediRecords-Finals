using System;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Dto.AllergyDtos;
using MediRecords.Repository.AllergyRepository;
using MediRecords.Repository.PatientRepo;
using MediRecords.Services.AuthServices;
using MediRecords.Utility;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace MediRecords.Services.AllergyServices;

public class AllergyService : IAllergyService
{
    private readonly IPatientRepository _patientRepo;
    private readonly IAllergyRepository _allergyRepo;
    private readonly IAuthService _authService;
    public AllergyService(IPatientRepository patientRepo, IAllergyRepository allergyRepo, IAuthService authService)
    {
        _patientRepo = patientRepo;
        _allergyRepo = allergyRepo;
        _authService = authService;
    }
    public async Task CreateAllergyAsync(int patientId, AllergyCreateRequestDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Allergen))
            throw new MediRecordsException(Constant.AllergenRequired);

        // check if patient record exists or not
        var patient = await _patientRepo.GetByIdWithDetailsAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException(Constant.PatientMessages.PatientNotFound);
        
        // Check only the assigned primary provider is dealing/accessing the patient
        _authService.EnsurePrimaryProviderAccess(patient, userId);

        // Checks if allergy already exists in database (prevents duplicacy)
        var exists = patient.Allergies.Any(a =>
            a.Allergen == dto.Allergen && a.Status == AllergyStatus.Active);

        if (exists)
            throw new MediRecordsException(Constant.AllergyExists);

        var allergy = new Allergy
        {
            PatientId = patientId,
            Allergen = dto.Allergen.Trim(),
            Reaction = dto.Reaction,
            Severity = dto.Severity,
            Status = AllergyStatus.Active,
            NotedDate = DateTime.UtcNow
        };

        await _allergyRepo.AddAsync(allergy);

        await _authService.SaveAuditLog(
            userId,
            $"ADD_ALLERGY:{patientId}"
        );
    }
}
