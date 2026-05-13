using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("Immunization")]
public class Immunization
{
    [Key]
    public int ImmunizationId { get; set; }

    [Required]
    [ForeignKey("PatientIdNavigation")]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Vaccine { get; set; } = null!;

    [MaxLength(50)]
    public string Dose { get; set; } = null!;

    public DateTime GivenDate { get; set; }

    // 0: Pending, 1: Administered
    public bool Status { get; set; }

    public virtual Patient? PatientIdNavigation { get; set; }
}