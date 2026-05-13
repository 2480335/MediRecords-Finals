using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Enums;

namespace MediRecords.Domain.Entities;

[Table("Document")]
public class Document
{
    [Key]
    public int DocumentID { get; set; }

    [Required]
    [ForeignKey("PatientIdNavigation")]
    public int PatientID { get; set; }

    [Required]
    [ForeignKey("EncounterIdNavigation")]
    public int EncounterID { get; set; }

    [Required]
    public DocumentType DocType { get; set; }

    [MaxLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? FileName { get; set; }

    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string? FileType { get; set; } // MIME type: application/pdf, image/jpeg, etc.

    [Column(TypeName = "varbinary(max)")]
    public byte[]? FileData { get; set; } // Binary file content

    [Required]
    public int UploadedBy { get; set; }

    [Required]
    [Column(TypeName = "datetime")]
    public DateTime UploadedDate { get; set; }

    [Required]
    public DocumentStatus Status { get; set; } = DocumentStatus.Active;

    public bool ProviderOnlyVisibility { get; set; } = false;

    public virtual Patient? PatientIdNavigation { get; set; }
    public virtual Encounter? EncounterIdNavigation { get; set; }
}