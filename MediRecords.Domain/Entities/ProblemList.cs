using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Enums;

namespace MediRecords.Domain.Entities;


[Table("ProblemList")]
public class ProblemList
{
    [Key]
    public int ProblemId { get; set; }

    [Required]
    [ForeignKey("PatientIdNavigation")]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Diagnosis { get; set; } = null!;

    public ProblemStatus Status { get; set; } = ProblemStatus.Active; // Active, Resolved, Chronic

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Patient? PatientIdNavigation { get; set; }
}
