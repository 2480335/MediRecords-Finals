using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Entities;

namespace MediRecords.Models;

[Table("VitalSign")]
public class VitalSign
{
     [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VitalsId { get; set; }

    [Required]
    public int EncounterId { get; set; }

    [MaxLength(20)]
    public string? BP { get; set; } 

    public double? HR { get; set; }
    
    public double? Temp { get; set; }

    public double? SpO2 { get; set; }

    public double? Height { get; set; }

    public double? Weight { get; set; }

    public double? BMI { get; set; }

    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string RecordedBy { get; set; } = null!;

    [ForeignKey("EncounterId")]
    public virtual Encounter? EncounterIdNavigation { get; set; }

}