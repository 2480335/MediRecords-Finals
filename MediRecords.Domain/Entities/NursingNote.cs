using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Entities;

namespace MediRecords.Models;

[Table("NursingNote")]
public class NursingNote
{   
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int NursingNoteId { get; set; } //NursingNoteId Primary Key for Note IDs
    
    [Required]
    [ForeignKey("EncounterNavigation")]
    public int EncounterId { get; set; } // IDs for every Encounters

    [Column(TypeName ="VARCHAR(50)")]
    public string? Notes { get; set; } // Notes By the nurse.

    [Required]
    [Column(TypeName ="VARCHAR(50)")]
    public string RecordedBy { get; set; } = null!; // Name of the person who recorded this note 

    public DateTime RecordedDate { get; set; } // Date of the Note record.

    // Foreign Keys
    public virtual Encounter? EncounterNavigation { get; set; }
}