using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Enums;

namespace MediRecords.Domain.Entities;

[Table("Appointment")]
public class Appointment
{
    [Key]
    public int AppointmentId { get; set; }

    [Required]
    [ForeignKey("PatientIdNavigation")]
    public int PatientId { get; set; }

    [Required]
    [ForeignKey("ProviderIdNavigation")]
    public int ProviderId { get; set; }

    [Required]
    [Column(TypeName = "datetime")]
    public DateTime DateTime { get; set; }

    [MaxLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? Reason { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; } 

    public virtual User? ProviderIdNavigation { get; set; }
    public virtual Patient? PatientIdNavigation { get; set; }
}