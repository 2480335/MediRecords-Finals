using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Dto.AppointmentsDtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MediRecords.Repository
{
    public class AppointmentsRepository : IAppointmentsRepository
    {
        private readonly MediRecordsDbContext _context;

        public AppointmentsRepository(MediRecordsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PatientExistsAsync(int patientId)
        {
            return await _context.Patients.AnyAsync(p => p.PatientId == patientId);
        }

        public async Task<bool> ProviderExistsAsync(int providerId)
        {
            return await _context.ProviderSchedules.AnyAsync(pr => pr.ProviderId == providerId);
        }

        public async Task<bool> SlotExistsAsync(int providerId, DateTime dateTime)
        {
            return await _context.Appointments.AsNoTracking()
                .AnyAsync(a =>
                    a.ProviderId == providerId &&
                    a.DateTime == dateTime &&
                    a.Status == AppointmentStatus.Booked); // since Status is bool in entity
        }


        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
