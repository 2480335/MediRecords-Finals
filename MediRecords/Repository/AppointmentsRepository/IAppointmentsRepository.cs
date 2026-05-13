using MediRecords.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace MediRecords.Repository
{
    public interface IAppointmentsRepository
    {
        Task<bool> PatientExistsAsync(int patientId);
        Task<bool> ProviderExistsAsync(int providerId);
        Task<bool> SlotExistsAsync(int providerId, DateTime dateTime);
        Task AddAsync(Appointment appointment);
        Task SaveChangesAsync();
    }
}
