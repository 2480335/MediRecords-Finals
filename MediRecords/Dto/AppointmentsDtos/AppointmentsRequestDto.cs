using System;
using System.ComponentModel.DataAnnotations;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.AppointmentsDtos
{
    public class AppointmentsRequestDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public string? Reason { get; set; }
    }
}
