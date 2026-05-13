using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Xsl;
using MediRecords.Domain.Enums;

namespace MediRecords.Domain.Entities;

[Table("ImagingOrder")]
public class ImagingOrder
{
    [Key]
    public int ImagingOrderId { get; set; }

    [Required]
    [ForeignKey("EncounterIdNavigation")]
    public int EncounterId { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public ImagingOrderStudyType StudyType { get; set; }

    [Column(TypeName = "VARCHAR(MAX)")]
    public string? Notes { get; set; }

    public DateTime OrderedDate { get; set; }

    public bool Status { get; set; }

    /*-------------------------Foreign Key References--------------------------------*/
    public virtual ICollection<ImagingReport> ImagingReports { get; set; } = new List<ImagingReport>();
    public virtual Encounter? EncounterIdNavigation { get; set; }

}
