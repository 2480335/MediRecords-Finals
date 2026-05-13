using MediRecords.Domain.Entities;

namespace MediRecords.Repository.PrescriptionRepository;

public interface IPrescriptionRepository
{
    Task<int> GetPrescriptionCountAsync(DateTime fromDate, DateTime toDate);
}