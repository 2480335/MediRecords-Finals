using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Enums;

namespace MediRecords.Domain.Entities;

[Table("Patient")]
public class Patient
{
    [Key]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(50)]
    public string MRN { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    public DateOnly DOB { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    public string? AddressJSON { get; set; }

    [MaxLength(255)]
    public string? ContactInfo { get; set; }
    
    [MaxLength(15)]
    public string PhoneNo { get; set; } = null!;

    [Required]
    public int CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PrimaryProviderIdNavigation")]
    public int? PrimaryProviderId { get; set; }

    [MaxLength(20)]
    public PatientStatus Status { get; set; } = PatientStatus.Active;

    public virtual ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
    public virtual ICollection<Immunization> Immunizations { get; set; } = new List<Immunization>();
    public virtual ICollection<MedicationList> MedicationLists { get; set; } = new List<MedicationList>();
    public virtual ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
    public virtual ICollection<ProblemList> ProblemLists { get; set; } = new List<ProblemList>();

    public virtual User? PrimaryProviderIdNavigation { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public virtual User CreatedByUserNavigation { get; set; } = null!;
}
