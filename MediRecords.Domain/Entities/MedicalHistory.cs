using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("MedicalHistory")]
public class MedicalHistory
{
    [Key]
    public int HistoryId { get; set; }

    [Required]
    [ForeignKey("PatientIdNavigation")]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Condition { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime RecordedDate { get; set; } = DateTime.Now;

    public virtual Patient? PatientIdNavigation { get; set; }
}
