using System;
using MediRecords.Dto.EncounterDtos.Request;
using MediRecords.Dto.EncounterDtos.Response;

namespace MediRecords.Services.EncounterServices;

public interface IEncounterService
{
    Task<IEnumerable<EncounterSummaryDto>> GetWorkspaceAsync(int providerId, DateTime? date);
    Task<EncounterDetailDto?> GetEncounterByIdAsync(int encounterId);
    Task<(bool Success, string Message, EncounterStatusResponseDto? Data)> UpdateEncounterStatusAsync(int encounterId, EncounterStatusUpdateDto dto);
    Task<IEnumerable<EncounterLookupDto>> GetAllEncountersAsync();
}
