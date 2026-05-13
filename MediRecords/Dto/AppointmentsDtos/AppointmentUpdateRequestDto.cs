using System;

namespace MediRecords.Dto.AppointmentDtos
{
    public class AppointmentUpdateRequestDto
    {
        public int? PatientId { get; set; }
        public int? ProviderId { get; set; }
        public DateTime? DateTime { get; set; }
        public string? Reason { get; set; }
        // Status always comes from path parameter, not here
    }
}