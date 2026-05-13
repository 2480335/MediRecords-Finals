using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using MediRecords.Domain.Enums;
using MediRecords.Models;

namespace MediRecords.Domain.Entities;

[Table("Encounter")]
public class Encounter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int EncounterId { get; set; }
    
    [ForeignKey("PatientIdNavigation")]
    public int PatientId { get; set; }

    [ForeignKey("ProviderIdNavigation")]
    public int ProviderId { get; set; }

    public DateTime Date { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(MAX)")]
    public string VisitType { get; set; } = null!;
    
    public EncounterStatus Status { get; set; }

    public virtual ICollection<SOAPNote> SOAPNotes { get; set; } = new List<SOAPNote>();

    public virtual ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
    public virtual ICollection<NursingNote> NursingNotes { get; set; } = new List<NursingNote>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    public virtual ICollection<ImagingOrder> ImagingOrders { get; set; } = new List<ImagingOrder>();
    public virtual ICollection<VisitChargeRef> VisitChargeRefs { get; set; } = new List<VisitChargeRef>();
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual Patient? PatientIdNavigation { get; set; }
    public virtual User? ProviderIdNavigation { get; set; }
}
