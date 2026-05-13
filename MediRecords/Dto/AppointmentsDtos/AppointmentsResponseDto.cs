using System;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.AppointmentsDtos
{
    public class AppointmentsResponseDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public DateTime DateTime { get; set; }
        public string? Reason { get; set; }
        public AppointmentStatus Status { get; set; } 
        public string Message { get; set; } 
    }
}
