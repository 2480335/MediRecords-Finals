using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Enums;

namespace MediRecords.Domain.Entities;

[Table("Allergy")]
public class Allergy
{
    [Key]
    public int AllergyId { get; set; }

    [Required]
    [ForeignKey("PatientIdNavigation")]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Allergen { get; set; } = null!;

    public string? Reaction { get; set; }

    [MaxLength(50)]
    public string? Severity { get; set; }

    public AllergyStatus Status { get; set; } = AllergyStatus.Active;

    public DateTime NotedDate { get; set; } = DateTime.Now;

    public virtual Patient? PatientIdNavigation { get; set; }
}
