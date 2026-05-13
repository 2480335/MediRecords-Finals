using System;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ImmunizationDtos;

namespace MediRecords.Services.ImmunizationService;

public interface IImmunizationService
{
    Task<int> CreateImmunizationAsync(int patientId, ImmunizationCreateRequestDto dto, int physicianId);
    Task<ImmunizationDetailsDto> GetByIdAsync(int immunizationId);
    Task<IEnumerable<ImmunizationDetailsDto>> GetImmunizationsAsync(int? patientId, string? patientName, string? vaccine, bool? status);
}
