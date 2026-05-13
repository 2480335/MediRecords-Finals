using System;
using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.AppointmentDtos
{
    public class AppointmentUpdateResponseDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public DateTime DateTime { get; set; }
        public string? Reason { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Populated when a check-in auto-creates a new encounter.
        /// Null when status update did not result in an encounter.
        /// </summary>
        public int? EncounterId { get; set; }
    }

    public static class AppointmentUpdateResponseExtension
    {
        public static AppointmentUpdateResponseDto ToAppointmentUpdateResponse(this Appointment appointment, int? encounterId = null)
        {
            return new AppointmentUpdateResponseDto
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                ProviderId = appointment.ProviderId,
                DateTime = appointment.DateTime,
                Reason = appointment.Reason,
                Status = appointment.Status,
                Message = "Status updated successfully",
                EncounterId = encounterId
            };
        }
    }
}
