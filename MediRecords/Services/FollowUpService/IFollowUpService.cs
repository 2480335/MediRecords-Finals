using System;
using MediRecords.Dto.FollowUpDtos;

namespace MediRecords.Services.FollowUpService;

public interface IFollowUpService
{
    Task<int> CreateFollowUpAsync(int encounterId, FollowUpCreateRequestDto dto, int physicianId);
    Task<IEnumerable<FollowUpDetailsDto>> GetFollowUpsAsync(int? patientId, int? encounterId);
    Task<FollowUpDetailsDto> GetByIdAsync(int followupId);
}
