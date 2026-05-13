using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.MedicalHistoryRepository;

public interface IMedicalHistoryRepository
{
    Task AddAsync(MedicalHistory history);
}
