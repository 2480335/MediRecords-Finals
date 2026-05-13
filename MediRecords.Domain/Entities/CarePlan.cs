using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("CarePlan")]
public class CarePlan
{
    [Key]
    public int CarePlanId { get; set; }

    [Required]
    [ForeignKey("PatientIdNavigation")]
    public int PatientId { get; set; }

    [Column(TypeName = "VARCHAR(MAX)")]
    public string GoalsJSON { get; set; } = null!;

    [Column(TypeName = "VARCHAR(MAX)")]
    public string Instructions { get; set; } = null!;

    // 0: Active, 1: Completed
    public bool Status { get; set; }

    public virtual Patient? PatientIdNavigation { get; set; }
}