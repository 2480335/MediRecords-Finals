using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Dto.AppointmentDtos;
using MediRecords.Dto.AppointmentsDtos;
using MediRecords.Services.AppointmentsServices;
using MediRecords.Utility;

namespace MediRecords.Services.AppointmentsServices
{
    public class AppointmentsService : IAppointmentsService
    {
        private readonly MediRecordsDbContext _context;

        public AppointmentsService(MediRecordsDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------
        // BOOK APPOINTMENT
        // --------------------------------------------------
        public async Task<AppointmentsResponseDto> BookAppointmentAsync(
            AppointmentsRequestDto dto)
        {
            if (dto.DateTime < DateTime.Now)
                throw new ArgumentException(
                    Constant.AppointmentMessages.PastDateNotAllowed);

            if (dto.PatientId <= 0)
                throw new ArgumentException(Constant.PatientIdRequired);

            if (dto.ProviderId <= 0)
                throw new ArgumentException(Constant.ProviderIdRequired);

            bool patientExists = await _context.Patients
                .AnyAsync(p => p.PatientId == dto.PatientId);

            if (!patientExists)
                throw new ArgumentException(Constant.PatientIdRequired);

            bool providerExists = await _context.Users
                .AnyAsync(u => u.UserId == dto.ProviderId);

            if (!providerExists)
                throw new ArgumentException(Constant.ProviderIdRequired);

            // Soft schedule check: only enforce ProviderSchedule when the provider
            // has configured at least one row. Providers with no schedule rows are
            // treated as always-available so booking isn't blocked while there's no
            // schedule-management UI/API. When schedules are added later, they
            // start being enforced automatically.
            const int DefaultSlotDurationMinutes = 30;
            int slotDuration;

            bool providerHasAnySchedule = await _context.ProviderSchedules
                .AnyAsync(ps => ps.ProviderId == dto.ProviderId);

            if (providerHasAnySchedule)
            {
                var schedule = await _context.ProviderSchedules
                    .FirstOrDefaultAsync(ps =>
                        ps.ProviderId == dto.ProviderId &&
                        ps.StartTime.Date == dto.DateTime.Date &&
                        ps.Status == true &&
                        dto.DateTime >= ps.StartTime &&
                        dto.DateTime.AddMinutes(ps.SlotDuration) <= ps.EndTime
                    );

                if (schedule == null)
                    throw new InvalidOperationException(
                        Constant.AppointmentMessages.ProviderNotAvailable);

                slotDuration = schedule.SlotDuration;
            }
            else
            {
                slotDuration = DefaultSlotDurationMinutes;
            }

            bool slotTaken = await _context.Appointments.AnyAsync(a =>
                a.ProviderId == dto.ProviderId &&
                a.Status == AppointmentStatus.Booked &&
                dto.DateTime < a.DateTime.AddMinutes(slotDuration) &&
                dto.DateTime.AddMinutes(slotDuration) > a.DateTime
            );

            if (slotTaken)
                throw new InvalidOperationException(
                    Constant.AppointmentMessages.ProviderSlotUnavailable);

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                ProviderId = dto.ProviderId,
                DateTime = dto.DateTime,
                Reason = dto.Reason,
                Status = AppointmentStatus.Booked
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new AppointmentsResponseDto
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                ProviderId = appointment.ProviderId,
                DateTime = appointment.DateTime,
                Reason = appointment.Reason,
                Status = appointment.Status,
                Message = Constant.Messages.Success
            };
        }

        // --------------------------------------------------
        // GET APPOINTMENTS (FILTERED)
        // --------------------------------------------------
        public async Task<List<AppointmentsResponseDto>> GetAppointmentsAsync(
            int? id,
            int? patientId,
            int? providerId,
            string? date)
        {
            IQueryable<Appointment> query =
                _context.Appointments.AsNoTracking();

            if (id.HasValue)
                query = query.Where(a => a.AppointmentId == id.Value);

            if (patientId.HasValue)
                query = query.Where(a => a.PatientId == patientId.Value);

            if (providerId.HasValue)
                query = query.Where(a => a.ProviderId == providerId.Value);

            if (!string.IsNullOrWhiteSpace(date))
            {
                if (int.TryParse(date, out int year))
                {
                    query = query.Where(a => a.DateTime.Year == year);
                }
                else if (DateTime.TryParse(date, out DateTime parsed))
                {
                    query = query.Where(a =>
                        a.DateTime.Date == parsed.Date);
                }
            }

            var appointments = await query.ToListAsync();

            return appointments.Select(a => new AppointmentsResponseDto
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                ProviderId = a.ProviderId,
                DateTime = a.DateTime,
                Reason = a.Reason,
                Status = a.Status,
                Message = "Fetched successfully"
            }).ToList();
        }

        // --------------------------------------------------
        // UPDATE APPOINTMENT
        // --------------------------------------------------
        public async Task<AppointmentUpdateResponseDto?> UpdateAppointmentAsync(
            int id,
            AppointmentStatus newStatus,
            AppointmentUpdateRequestDto request)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return null;

            // Enforce a proper state machine instead of the old
            // "block-everything-if-checked-in" guard which prevented
            // legitimate CheckedIn -> Completed / Cancelled / NoShow flows.
            ValidateStatusTransition(appointment.Status, newStatus);

            // Detect whether this is a booking modification (anything that
            // changes WHEN or WHO the appointment is for) vs a status-only
            // update. The slot/schedule sanity checks below only apply to
            // booking modifications — they don't make sense when simply
            // marking a past appointment as Completed / Cancelled / NoShow.
            bool isBookingModification =
                request.DateTime.HasValue ||
                request.PatientId.HasValue ||
                request.ProviderId.HasValue ||
                newStatus == AppointmentStatus.Booked;

            // ✅ Patient validation
            if (request.PatientId.HasValue)
            {
                bool patientExists = await _context.Patients
                    .AnyAsync(p => p.PatientId == request.PatientId.Value);

                if (!patientExists)
                    throw new ArgumentException(Constant.PatientIdRequired);

                appointment.PatientId = request.PatientId.Value;
            }

            int providerId = request.ProviderId ?? appointment.ProviderId;
            DateTime appointmentTime =
                request.DateTime ?? appointment.DateTime;

            // ✅ Provider validation
            if (request.ProviderId.HasValue)
            {
                bool providerExists = await _context.Users
                    .AnyAsync(u => u.UserId == request.ProviderId.Value);

                if (!providerExists)
                    throw new ArgumentException(Constant.ProviderIdRequired);

                appointment.ProviderId = request.ProviderId.Value;
            }

            // The remaining checks only apply when something about WHEN/WHO is
            // being changed. Status-only transitions (e.g. CheckedIn->Completed)
            // skip past-date / schedule / slot-conflict validation.
            if (isBookingModification)
            {
                // ✅ Date validation — only enforced when DateTime is being changed
                if (request.DateTime.HasValue &&
                    request.DateTime.Value < DateTime.Now)
                    throw new ArgumentException(
                        Constant.AppointmentMessages.PastDateNotAllowed);

                // ✅ Schedule validation — soft check: only enforce when the
                // provider has at least one ProviderSchedule row. See BookAppointmentAsync.
                const int DefaultSlotDurationMinutes = 30;
                int slotDuration;

                bool providerHasAnySchedule = await _context.ProviderSchedules
                    .AnyAsync(ps => ps.ProviderId == providerId);

                if (providerHasAnySchedule)
                {
                    var schedule = await _context.ProviderSchedules
                        .FirstOrDefaultAsync(ps =>
                            ps.ProviderId == providerId &&
                            ps.StartTime.Date == appointmentTime.Date &&
                            ps.Status == true &&
                            appointmentTime >= ps.StartTime &&
                            appointmentTime.AddMinutes(ps.SlotDuration) <= ps.EndTime
                        );

                    if (schedule == null)
                        throw new InvalidOperationException(
                            Constant.AppointmentMessages.ProviderNotAvailable);

                    slotDuration = schedule.SlotDuration;
                }
                else
                {
                    slotDuration = DefaultSlotDurationMinutes;
                }

                // ✅ Slot conflict
                bool slotTaken = await _context.Appointments.AnyAsync(a =>
                    a.AppointmentId != appointment.AppointmentId &&
                    a.ProviderId == providerId &&
                    a.Status == AppointmentStatus.Booked &&
                    appointmentTime < a.DateTime.AddMinutes(slotDuration) &&
                    appointmentTime.AddMinutes(slotDuration) > a.DateTime
                );

                if (slotTaken)
                    throw new InvalidOperationException(
                        Constant.AppointmentMessages.ProviderSlotUnavailable);

                appointment.DateTime = appointmentTime;
            }

            if (!string.IsNullOrWhiteSpace(request.Reason))
                appointment.Reason = request.Reason;

            appointment.Status = newStatus;

            // Auto-create an Encounter the first time an appointment is checked in.
            // The appointment row + the new encounter row are persisted in a single
            // SaveChangesAsync call so they commit (or roll back) atomically.
            int? encounterId = null;
            if (newStatus == AppointmentStatus.CheckedIn)
            {
                encounterId = await EnsureEncounterForCheckInAsync(appointment);
            }

            await _context.SaveChangesAsync();

            return appointment.ToAppointmentUpdateResponse(encounterId);
        }

        // --------------------------------------------------
        // ENCOUNTER AUTO-CREATION ON CHECK-IN
        // Returns the encounter id (existing or newly created).
        // --------------------------------------------------
        private async Task<int> EnsureEncounterForCheckInAsync(Appointment appointment)
        {
            // Idempotency guard: reuse if an encounter already exists for this
            // patient/provider on the appointment's day. Real long-term fix is
            // an AppointmentId foreign key on Encounter for an exact link.
            var apptDay = appointment.DateTime.Date;
            var nextDay = apptDay.AddDays(1);

            var existing = await _context.Encounters
                .Where(e => e.PatientId == appointment.PatientId &&
                            e.ProviderId == appointment.ProviderId &&
                            e.Date >= apptDay &&
                            e.Date < nextDay)
                .Select(e => (int?)e.EncounterId)
                .FirstOrDefaultAsync();

            if (existing.HasValue) return existing.Value;

            // TODO: change Encounter.EncounterId to IDENTITY(1,1) via migration
            // to remove this concurrency window. Until then, generate the next
            // id with MAX + 1. Two simultaneous check-ins could collide; the
            // DB unique-PK constraint will surface that as a 500.
            int nextId = (await _context.Encounters.MaxAsync(e => (int?)e.EncounterId) ?? 0) + 1;

            var encounter = new Encounter
            {
                EncounterId = nextId,
                PatientId   = appointment.PatientId,
                ProviderId  = appointment.ProviderId,
                Date        = DateTime.Now,
                VisitType   = !string.IsNullOrWhiteSpace(appointment.Reason)
                                ? appointment.Reason!
                                : "Consultation",
                Status      = EncounterStatus.Open
            };

            _context.Encounters.Add(encounter);
            return nextId;
        }

        // --------------------------------------------------
        // APPOINTMENT STATE MACHINE
        // --------------------------------------------------
        // Allowed transitions:
        //   Booked    -> CheckedIn | Cancelled | NoShow
        //   CheckedIn -> Completed | Cancelled | NoShow
        //   (Completed | Cancelled | NoShow are terminal)
        //
        // Re-applying the same status is rejected as a no-op.
        // --------------------------------------------------
        private static void ValidateStatusTransition(
            AppointmentStatus current,
            AppointmentStatus next)
        {
            if (current == next)
            {
                throw new InvalidOperationException(
                    string.Format(Constant.AppointmentMessages.AlreadyInStatus, current));
            }

            // Terminal states are write-once
            if (current is AppointmentStatus.Completed
                       or AppointmentStatus.Cancelled
                       or AppointmentStatus.NoShow)
            {
                throw new InvalidOperationException(
                    string.Format(Constant.AppointmentMessages.TerminalStatus, current));
            }

            // CheckedIn cannot revert to Booked
            if (current == AppointmentStatus.CheckedIn && next == AppointmentStatus.Booked)
            {
                throw new InvalidOperationException(
                    Constant.AppointmentMessages.CannotRevertCheckIn);
            }
        }
    }
}