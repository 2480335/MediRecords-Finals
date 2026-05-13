using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.FollowUpRepository;

public interface IFollowUpRepository
{
    Task AddFollowUpAsync(FollowUp followUp);
    Task<FollowUp?> GetByIdAsync(int followupId);
    Task<IEnumerable<FollowUp>> GetAsync(int? patientId, int? encounterId);
}
