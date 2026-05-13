using System;
using System.ClientModel.Primitives;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ImmunizationDtos;
using MediRecords.Repositories;
using MediRecords.Repository.ImmunizationRepository;
using MediRecords.Repository.PatientRepo;
using MediRecords.Services.AuthServices;
using MediRecords.Utility;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace MediRecords.Services.ImmunizationService;

public class ImmunizationService : IImmunizationService
{
    private readonly IImmunizationRepository _immunizationRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IAuthService _authService;
    public ImmunizationService(IImmunizationRepository immunizationRepo, IAuthService authService, IPatientRepository patientRepo)
    {
        _immunizationRepo = immunizationRepo;
        _authService = authService;
        _patientRepo = patientRepo;
    }
    public async Task<int> CreateImmunizationAsync(int patientId, ImmunizationCreateRequestDto dto, int physicianId)
    {
        // Fields Validation
        if(string.IsNullOrWhiteSpace(dto.Vaccine))
        {
            throw new MediRecordsException(Constant.RequiredFields);
        }

        if(string.IsNullOrWhiteSpace(dto.Dose))
        {
            throw new MediRecordsException(Constant.RequiredFields);
        }
        
        if(dto.GivenDate > DateTime.UtcNow)
        {
            throw new MediRecordsException(Constant.FutureDate);
        }

        var patient = await _patientRepo.GetByIdWithDetailsAsync(patientId);
        if(patient == null)
        {
            throw new KeyNotFoundException(Constant.PatientMessages.PatientNotFound);
        }

        // Only primary provider can access
        _authService.EnsurePrimaryProviderAccess(patient, physicianId);

        var immunization = new Immunization
        {
            PatientId = patientId,
            Vaccine = dto.Vaccine.Trim(),
            Dose = dto.Dose.Trim(),
            GivenDate = dto.GivenDate,
            Status = dto.Status
        };

        // Persist in the database
        await _immunizationRepo.AddAsync(immunization);

        await _authService.SaveAuditLog(
            physicianId,
            $"ADD_IMMUNIZATION:ImmunizationId: {immunization.ImmunizationId}, PatientId: {patientId}"
        );
        return immunization.ImmunizationId; 
    }

    public async Task<IEnumerable<ImmunizationDetailsDto>> GetImmunizationsAsync(int? patientId, string? patientName, string? vaccine, bool? status)
    {
        var list = await _immunizationRepo.GetAsync(patientId, patientName, vaccine, status);

        return list.Select(i => new ImmunizationDetailsDto
        {
            ImmunizationId = i.ImmunizationId,
            PatientId = i.PatientId,
            PatientName = i.PatientIdNavigation!.Name,
            Vaccine = i.Vaccine,
            Dose = i.Dose,
            GivenDate = i.GivenDate,
            Status = i.Status
        });
    }

    public async Task<ImmunizationDetailsDto> GetByIdAsync(int immunizationId)
    {
        if(immunizationId <= 0)
        {
            throw new ArgumentException("Invalid immunization id");
        }

        var immunization = await _immunizationRepo.GetByIdAsync(immunizationId);
        if (immunization == null)
        {
            throw new MediRecordsException($"Immunization with {immunizationId} not found.");
        }

        return new ImmunizationDetailsDto
        {
            ImmunizationId = immunization.ImmunizationId,
            PatientId = immunization.PatientId,
            PatientName = immunization.PatientIdNavigation!.Name,
            Vaccine = immunization.Vaccine,
            Dose = immunization.Dose,
            GivenDate = immunization.GivenDate,
            Status = immunization.Status
        };
    }
}
