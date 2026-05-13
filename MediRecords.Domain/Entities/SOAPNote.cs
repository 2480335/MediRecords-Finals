using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MediRecords.Domain.Entities;

[Table("SOAPNote")]
public class SOAPNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NoteId { get; set; }
    
    [Required]
    [ForeignKey("EncounterIdNavigation")]
    public int EncounterId { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Subjective { get; set; } = null!;

    [Column(TypeName = "nvarchar(max)")]
    public string? Objective { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Assessment { get; set; }

    [Column( TypeName = "nvarchar(max)")]
    public string? Plan { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool Status { get; set; }
    
    public virtual Encounter? EncounterIdNavigation { get; set; }
}